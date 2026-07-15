CREATE TABLE [dbo].[stockDataReport] (
    [stockDataReportId] UNIQUEIDENTIFIER CONSTRAINT [DF_stockDataReport_stockDataReportId] DEFAULT (newid()) NOT NULL,
    [tickerSymbol]      VARCHAR (10)     NOT NULL,
    [httpResult]        INT              NOT NULL,
    [message]           VARCHAR (MAX)    NULL,
    CONSTRAINT [PK_stockDataReport] PRIMARY KEY CLUSTERED ([stockDataReportId] ASC)
);

