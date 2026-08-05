namespace SwineBot.Updates;

public enum UpdateHandleResult 
{ 
    // Message
    MessageOK, 
    MessageForward, 
    MessageNotCommand, 

    // Inline query
    InlineQueryOK, 
    InlineQueryWrongChatType,
    InlineQueryGroupIdNotLong,

    // General
    SendMessageFail,
    DatabaseFail, 
    OtherUpdate,
    InlineQueryGroupNotFound,
    InlineQueryUserNotFound,
    InlineQuerySwineNotFound,
    InlineQueryInvalidFormat,
    MessageUnknownCommand,
    UnknownError,
    MessageFailedToCreateUpdate,
    CommandFailed,
    MessageSuccesfulMigration,
}

