CREATE TABLE [dbo].[ticker] (
    [tickerSymbol]   VARCHAR (50)  NOT NULL,
    [description]    VARCHAR (100) NULL,
    [exchangeSymbol] VARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_ticker] PRIMARY KEY CLUSTERED ([tickerSymbol] ASC)
);

