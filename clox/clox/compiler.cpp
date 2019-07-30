#include <cstdio>
#include <cstdlib>

#include "common.h"
#include "compiler.h"
#include "scanner.h"

#ifdef DEBUG_PRINT_CODE
#include "debug.h"
#endif

struct Parser
{
    ScannerState* scanner;
    Chunk* compiling_chunk;
    Token current;
    Token previous;
    bool had_error;
    bool panic_mode;
};

enum Precedence
{
    PREC_NONE,
    PREC_ASSIGNMENT,  // =        
    PREC_OR,          // or       
    PREC_AND,         // and      
    PREC_EQUALITY,    // == !=    
    PREC_COMPARISON,  // < > <= >=
    PREC_TERM,        // + -      
    PREC_FACTOR,      // * /      
    PREC_UNARY,       // ! -      
    PREC_CALL,        // . () []  
    PREC_PRIMARY
};

typedef void (*ParseFn)(Parser&);

struct ParseRule
{
    ParseFn prefix;
    ParseFn infix;
    Precedence precedence;
};

static void grouping(Parser&);
static void unary(Parser&);
static void binary(Parser&);
static void literal_val(Parser&);
static void number(Parser&);

static constexpr ParseRule PARSE_RULES[] =
{
    { grouping,    nullptr, PREC_NONE },       // TOKEN_LEFT_PAREN      
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_RIGHT_PAREN     
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_LEFT_BRACE
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_RIGHT_BRACE     
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_COMMA           
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_DOT             
    { unary,       binary,  PREC_TERM },       // TOKEN_MINUS           
    { nullptr,     binary,  PREC_TERM },       // TOKEN_PLUS            
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_SEMICOLON       
    { nullptr,     binary,  PREC_FACTOR },     // TOKEN_SLASH           
    { nullptr,     binary,  PREC_FACTOR },     // TOKEN_STAR            
    { unary,       nullptr, PREC_NONE },       // TOKEN_BANG            
    { nullptr,     binary,  PREC_EQUALITY },   // TOKEN_BANG_EQUAL      
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_EQUAL           
    { nullptr,     binary,  PREC_EQUALITY },   // TOKEN_EQUAL_EQUAL     
    { nullptr,     binary,  PREC_COMPARISON }, // TOKEN_GREATER         
    { nullptr,     binary,  PREC_COMPARISON }, // TOKEN_GREATER_EQUAL   
    { nullptr,     binary,  PREC_COMPARISON }, // TOKEN_LESS            
    { nullptr,     binary,  PREC_COMPARISON }, // TOKEN_LESS_EQUAL      
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_IDENTIFIER      
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_STRING          
    { number,      nullptr, PREC_NONE },       // TOKEN_NUMBER          
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_AND             
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_CLASS           
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_ELSE            
    { literal_val, nullptr, PREC_NONE },       // TOKEN_FALSE           
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_FOR             
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_FUN             
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_IF              
    { literal_val, nullptr, PREC_NONE },       // TOKEN_NIL             
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_OR              
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_PRINT           
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_RETURN          
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_SUPER           
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_THIS            
    { literal_val, nullptr, PREC_NONE },       // TOKEN_TRUE            
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_VAR             
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_WHILE           
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_ERROR           
    { nullptr,     nullptr, PREC_NONE },       // TOKEN_EOF
};

static constexpr const ParseRule& get_rule(TokenType type)
{
    return PARSE_RULES[type];
}

static void print_error(Parser& parser, const Token& token, const char* message)
{
    if (parser.panic_mode)
        return;
    parser.panic_mode = true;

    fprintf(stderr, "[line %d] Error", token.line);

    if (token.type == TOKEN_EOF)
        fprintf(stderr, " at end");
    else if (token.type != TOKEN_ERROR)
        fprintf(stderr, " at '%.*s'", token.length, token.start);

    fprintf(stderr, ": %s\n", message);
    parser.had_error = true;
}

static void error(Parser& parser, const char* message)
{
    print_error(parser, parser.previous, message);
}

static void error_at_current(Parser& parser, const char* message)
{
    print_error(parser, parser.current, message);
}

static void advance(Parser& parser)
{
    parser.previous = parser.current;
    for (;;)
    {
        parser.current = scan_token(*parser.scanner);
        if (parser.current.type != TOKEN_ERROR)
            break;
        error_at_current(parser, parser.current.start);
    }
}

static void consume(Parser& parser, TokenType type, const char* message)
{
    if (parser.current.type == type)
        advance(parser);
    else
        error_at_current(parser, message);
}

static Chunk* current_chunk(Parser& parser)
{
    return parser.compiling_chunk;
}

static uint8_t make_constant(Parser& parser, Value value)
{
    int constant = add_constant(*current_chunk(parser), value);
    if (constant > UINT8_MAX)
        error(parser, "Too many constants in one chunk");
    else
        return (uint8_t)constant;
}

static void emit_byte(Parser& parser, uint8_t value)
{
    write_chunk(*current_chunk(parser), value, parser.previous.line);
}

static void emit_bytes(Parser& parser, uint8_t v0, uint8_t v1)
{
    emit_byte(parser, v0);
    emit_byte(parser, v1);
}

static void emit_return(Parser& parser)
{
    emit_byte(parser, OP_RETURN);
}

static void emit_constant(Parser& parser, Value value)
{
    emit_bytes(parser, OP_CONSTANT, make_constant(parser, value));
}

static void end_compiler(Parser& parser)
{
    emit_return(parser);

#ifdef DEBUG_PRINT_CODE
    if (!parser.had_error)
        disassemble_chunk(*current_chunk(parser), "code");
#endif
}

static void parse_precedence(Parser& parser, Precedence precedence)
{
    advance(parser);
    ParseFn prefix_rule = get_rule(parser.previous.type).prefix;
    if (prefix_rule == nullptr)
        error(parser, "Expect expression");
    else
    {
        prefix_rule(parser);

        while (precedence <= get_rule(parser.current.type).precedence)
        {
            advance(parser);
            ParseFn infix_rule = get_rule(parser.previous.type).infix;
            if (infix_rule != nullptr)
                infix_rule(parser);
        }
    }
}

static void number(Parser& parser)
{
    double value = strtod(parser.previous.start, nullptr);
    emit_constant(parser, number_val(value));
}

static void unary(Parser& parser)
{
    TokenType operator_type = parser.previous.type;

    parse_precedence(parser, PREC_UNARY);

    switch (operator_type)
    {
    case TOKEN_BANG: emit_byte(parser, OP_NOT); break;
    case TOKEN_MINUS: emit_byte(parser, OP_NEGATE); break;

    default:
        return;
    }
}

static void binary(Parser& parser)
{
    TokenType operator_type = parser.previous.type;

    const ParseRule& rule = get_rule(operator_type);
    parse_precedence(parser, (Precedence)(rule.precedence + 1));

    switch (operator_type)
    {
    case TOKEN_BANG_EQUAL:    emit_bytes(parser, OP_EQUAL, OP_NOT); break;
    case TOKEN_EQUAL_EQUAL:   emit_byte(parser, OP_EQUAL); break;
    case TOKEN_GREATER:       emit_byte(parser, OP_GREATER); break;
    case TOKEN_GREATER_EQUAL: emit_bytes(parser, OP_LESS, OP_NOT); break;
    case TOKEN_LESS:          emit_byte(parser, OP_LESS); break;
    case TOKEN_LESS_EQUAL:    emit_bytes(parser, OP_GREATER, OP_NOT); break;
    case TOKEN_PLUS:          emit_byte(parser, OP_ADD); break;
    case TOKEN_MINUS:         emit_byte(parser, OP_SUBTRACT); break;
    case TOKEN_STAR:          emit_byte(parser, OP_MULTIPLY); break;
    case TOKEN_SLASH:         emit_byte(parser, OP_DIVIDE); break;

    default:
        return;
    }
}

static void literal_val(Parser& parser)
{
    switch (parser.previous.type)
    {
    case TOKEN_NIL:
        emit_byte(parser, OP_NIL);
        break;
    case TOKEN_FALSE:
        emit_byte(parser, OP_FALSE);
        break;
    case TOKEN_TRUE:
        emit_byte(parser, OP_TRUE);
        break;

    default:
        return;
    }
}

static void expression(Parser& parser)
{
    parse_precedence(parser, PREC_ASSIGNMENT);
}

static void grouping(Parser& parser)
{
    expression(parser);
    consume(parser, TOKEN_RIGHT_PAREN, "Expect ')' after expression");
}

bool compile(const char* source, Chunk& chunk)
{
    ScannerState scanner_state = {};
    init_scanner_state(scanner_state, source);

    Parser parser = {};
    parser.scanner = &scanner_state;
    parser.compiling_chunk = &chunk;

    advance(parser);
    expression(parser);
    consume(parser, TOKEN_EOF, "Expect end of expression");
    end_compiler(parser);

    return !parser.had_error;
}
