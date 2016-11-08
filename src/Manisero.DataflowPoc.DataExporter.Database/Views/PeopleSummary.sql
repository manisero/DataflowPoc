CREATE VIEW [dbo].[PeopleSummary] AS
SELECT
    (SELECT COUNT(*) FROM [Person]) AS [Count],
    (SELECT MIN(DateOfBirth) FROM [Person]) AS [DateOfBirthMin],
    (SELECT MAX(DateOfBirth) FROM [Person]) AS [DateOfBirthMax]
