CREATE TABLE [dbo].[Position] (
    [positionId]     INT             IDENTITY (1, 1) NOT NULL,
    [tickerSymbol]   VARCHAR (50)    NOT NULL,
    [exchangeSymbol] VARCHAR (50)    NOT NULL,
    [regime]         VARCHAR (50)    NOT NULL,
    [confidence]     DECIMAL (18, 3) NOT NULL,
    [purchasePrice]  DECIMAL (18, 3) NOT NULL,
    [numberOfShares] DECIMAL (18, 4) NOT NULL,
    [purchaseDate]   DATE            NOT NULL,
    [sellPrice]      DECIMAL (18, 3) NULL,
    [sellDate]       DATE            NULL,
    CONSTRAINT [PK_Position] PRIMARY KEY CLUSTERED ([positionId] ASC)
);
