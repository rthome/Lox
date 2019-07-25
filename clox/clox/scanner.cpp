#include <cstdio>
#include <cstring>

#include "common.h"
#include "scanner.h"

static bool is_at_end(ScannerState* state)
{
    return state->current == '\0';
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

static char peek(ScannerState* state)
{
    return *state->current;
}

static char peek_next(ScannerState* state)
{
    if (is_at_end(state))
        return '\0';
    return *(state->current + 1);
}

static char advance(ScannerState* state)
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
