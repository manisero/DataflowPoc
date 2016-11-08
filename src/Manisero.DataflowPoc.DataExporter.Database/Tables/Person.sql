CREATE TABLE [dbo].[Person] (
    [PersonId]            INT             IDENTITY (1, 1) NOT NULL,
    [FirstName]           NVARCHAR (100)  NOT NULL,
    [LastName]            NVARCHAR (100)  NOT NULL,
    [DateOfBirth]         DATE            NULL,
    [ArmToLegLengthRatio] FLOAT (53)      NULL,
    [PhoneNumber]         NVARCHAR (50)   NULL,
    [Salary]              DECIMAL (19, 4) NULL,
    CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED ([PersonId] ASC)
);

