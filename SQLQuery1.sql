CREATE TABLE [TotalStat]
(
	[id] INT IDENTITY PRIMARY KEY,
	[dead_percent] FLOAT,
	[recovered_percent] FLOAT
);

INSERT INTO [TotalStat] ([dead_percent], [recovered_percent]) VALUES (3.4, 54.0)