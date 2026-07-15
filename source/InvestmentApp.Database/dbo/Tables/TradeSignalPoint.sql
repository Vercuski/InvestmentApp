CREATE TABLE [dbo].[TradeSignalPoint] (
    [tradeSignalId] INT             IDENTITY (1, 1) NOT NULL,
    [tickerSymbol]  VARCHAR (50)    NOT NULL,
    [priceDate]     DATE            NOT NULL,
    [action]        VARCHAR (50)    NOT NULL,
    [regime]        VARCHAR (50)    NOT NULL,
    [confidence]    DECIMAL (18, 3) NOT NULL,
    [atrValue]      DECIMAL (18, 3) NOT NULL,
    [stopLossPrice] DECIMAL (18, 3) NULL
);

