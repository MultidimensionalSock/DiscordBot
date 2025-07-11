﻿namespace BotFramework.Structures
{
    public enum CloseEvent
    {
        UNKNOWN_ERROR = 4000,
        UNKNOWN_OPCODE = 4001,
        DECODE_ERROR = 4002,
        NOT_AUTHENTICATED = 4003,
        AUTHENTICATION_FAILED = 4004,
        ALREADY_AUTHENTICATED = 4005,
        INVALID_SEQ = 4007,
        RATE_LIMITED = 4008,
        SESSION_TIMED_OUT = 4009,
        INVALID_SHARD = 4010,
        INVALID_API_VERSION = 4012,
        INVALID_INTENT = 4013,
        DISALLOWED_INTENT = 4014
    }
}
