#include <cstdio>
#include <cstring>

#include "common.h"
#include "scanner.h"

static inline bool is_at_end(ScannerState* state)
{
    return *state->current == '\0';
}

static inline bool is_digit(char c)
{
    return c >= '0' && c <= '9';
}

static inline bool is_alpha(char c)
{
    return (c >= 'a' && c <= 'z')
        || (c >= 'A' && c <= 'Z')
        || c == '_';
}

static bool match(ScannerState* state, char expected)
{
    if (is_at_end(state))
        return false;
    if (*state->current != expected)
        return false;

    state->current++;
    return true;
}

static inline char peek(ScannerState* state)
{
    return *state->current;
}

static inline char peek_next(ScannerState* state)
{
    if (is_at_end(state))
        return '\0';
    return *(state->current + 1);
}

static inline char advance(ScannerState* state)
{
    state->current++;
    return *(state->current - 1);
}

static void whitespace(ScannerState* state)
{
    for (;;)
    {
        char c = peek(state);
        switch (c)
        {
        case ' ':
        case '\r':
        case '\t':
            advance(state);
            break;

        case '\n':
            advance(state);
            state->line++;
            break;

        case '/':
            if (peek_next(state) == '/')
            {
                while (peek(state) != '\n' && !is_at_end(state))
                    advance(state);
            }
            else
                return;
            break;

        default:
            return;
        }
    }
}

static Token make_token(ScannerState* state, TokenType type)
{
    int length = (int)(state->current - state->start);
    Token token = { type, state->start, length, state->line };
    return token;
}

static Token error_token(ScannerState* state, const char* message)
{
    Token token = { TOKEN_ERROR, message, (int)strlen(message), state->line };
    return token;
}

static TokenType check_keyword(ScannerState* state, int start, int length, const char* rest, TokenType type)
{
    if (state->current - state->start == start + length &&
        memcmp(state->start + start, rest, length) == 0)
        return type;
    return TOKEN_IDENTIFIER;
}

static TokenType identifier_type(ScannerState* state)
{
    switch (*state->start)
    {
    case 'a': return check_keyword(state, 1, 2, "nd", TOKEN_AND);
    case 'c': return check_keyword(state, 1, 4, "lass", TOKEN_CLASS);
    case 'e': return check_keyword(state, 1, 3, "lse", TOKEN_ELSE);
    case 'f':
        if (state->current - state->start > 1) {
            switch (*(state->start + 1))
            {
            case 'a': return check_keyword(state, 2, 3, "lse", TOKEN_FALSE);
            case 'o': return check_keyword(state, 2, 1, "r", TOKEN_FOR);
            case 'u': return check_keyword(state, 2, 1, "n", TOKEN_FUN);
            }
        }
        break;
    case 'i': return check_keyword(state, 1, 1, "f", TOKEN_IF);
    case 'n': return check_keyword(state, 1, 2, "il", TOKEN_NIL);
    case 'o': return check_keyword(state, 1, 1, "r", TOKEN_OR);
    case 'p': return check_keyword(state, 1, 4, "rint", TOKEN_PRINT);
    case 'r': return check_keyword(state, 1, 5, "eturn", TOKEN_RETURN);
    case 's': return check_keyword(state, 1, 4, "uper", TOKEN_SUPER);
    case 't':
        if (state->current - state->start > 1) {
            switch (*(state->start + 1))
            {
            case 'h': return check_keyword(state, 2, 2, "is", TOKEN_THIS);
            case 'r': return check_keyword(state, 2, 2, "ue", TOKEN_TRUE);
            }
        }
        break;
    case 'v': return check_keyword(state, 1, 2, "ar", TOKEN_VAR);
    case 'w': return check_keyword(state, 1, 4, "hile", TOKEN_WHILE);
    }

    return TOKEN_IDENTIFIER;
}

static Token identifier(ScannerState* state)
{
    while (is_alpha(peek(state)) || is_digit(peek(state)))
        advance(state);

    return make_token(state, identifier_type(state));
}

static Token number(ScannerState* state)
{
    while (is_digit(peek(state)))
        advance(state);

    if (peek(state) == '.' && is_digit(peek_next(state)))
    {
        advance(state);
        while (is_digit(peek(state)))
            advance(state);
    }

    return make_token(state, TOKEN_NUMBER);
}

static Token string(ScannerState* state)
{
    while (peek(state) != '"' && !is_at_end(state))
    {
        if (peek(state) == '\n')
            state->line++;
        advance(state);
    }

    if (is_at_end(state))
        return error_token(state, "Unterminated string");

    advance(state);
    return make_token(state, TOKEN_STRING);
}

void init_scanner_state(ScannerState* state, const char* source)
{
    state->start = source;
    state->current = source;
    state->line = 1;
}

Token scan_token(ScannerState* state)
{
    whitespace(state);

    state->start = state->current;

    if (is_at_end(state))
        return make_token(state, TOKEN_EOF);

    char c = advance(state);

    if (is_alpha(c))
        return identifier(state);
    if (is_digit(c))
        return number(state);

    switch (c)
    {
    case '(': return make_token(state, TOKEN_LEFT_PAREN);
    case ')': return make_token(state, TOKEN_RIGHT_PAREN);
    case '{': return make_token(state, TOKEN_LEFT_BRACE);
    case '}': return make_token(state, TOKEN_RIGHT_BRACE);
    case ';': return make_token(state, TOKEN_SEMICOLON);
    case ',': return make_token(state, TOKEN_COMMA);
    case '.': return make_token(state, TOKEN_DOT);
    case '-': return make_token(state, TOKEN_MINUS);
    case '+': return make_token(state, TOKEN_PLUS);
    case '/': return make_token(state, TOKEN_SLASH);
    case '*': return make_token(state, TOKEN_STAR);
    
    case '!':
        return make_token(state, match(state, '=') ? TOKEN_BANG_EQUAL : TOKEN_BANG);
    case '=':
        return make_token(state, match(state, '=') ? TOKEN_EQUAL_EQUAL : TOKEN_EQUAL);
    case '<':
        return make_token(state, match(state, '=') ? TOKEN_LESS_EQUAL : TOKEN_LESS);
    case '>':
        return make_token(state, match(state, '=') ? TOKEN_GREATER_EQUAL : TOKEN_GREATER);

    case '"':
        return string(state);
    }

    return error_token(state, "unexpected character");
}
