namespace SwineBot.Updates;

public enum UpdateHandleResult 
{ 
    // Message
    MessageOK, 
    MessageForward, 
    MessageNotCommand, 
    MessageUnknownCommand,
    MessageFailedToCreateUpdate,
    MessageSuccesfulMigration,

    // Inline query
    InlineQueryOK, 
    InlineQueryWrongChatType,
    InlineQueryGroupIdNotLong,
    InlineQueryGroupNotFound,
    InlineQueryUserNotFound,
    InlineQuerySwineNotFound,
    InlineQueryInvalidFormat,

    // Callback query
    CallbackQueryOK,
    CallbackQueryUnknownData,
    CallbackQueryInvalidDuelRequestId,

    // General
    SendMessageFail,
    DatabaseFail, 
    OtherUpdate,
    UnknownError,
    CommandFailed,
    CallbackQueryDuelRequestNotFound,
}

