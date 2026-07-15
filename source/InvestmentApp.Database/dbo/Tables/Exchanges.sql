CREATE TABLE [dbo].[Exchanges] (
    [exchangeSymbol]      VARCHAR (50)  NOT NULL,
    [exchangeDescription] VARCHAR (100) NOT NULL,
    [active]              BIT           CONSTRAINT [DF_Exchanges_active] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Exchanges] PRIMARY KEY CLUSTERED ([exchangeSymbol] ASC)
);

