CREATE TABLE [dbo].[stockData] (
    [stockDataId]  INT          IDENTITY (1, 1) NOT NULL,
    [tickerSymbol] VARCHAR (50) NOT NULL,
    [open]         MONEY        NOT NULL,
    [high]         MONEY        NOT NULL,
    [low]          MONEY        NOT NULL,
    [close]        MONEY        NOT NULL,
    [volume]       MONEY        NOT NULL,
    [date]         DATE         NOT NULL,
    CONSTRAINT [PK_stockData] PRIMARY KEY CLUSTERED ([stockDataId] ASC)
);

