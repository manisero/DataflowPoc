INSERT INTO [Person]
           ([FirstName]
           ,[LastName]
           ,[DateOfBirth]
           ,[ArmToLegLengthRatio]
           ,[PhoneNumber]
           ,[Salary])
SELECT [FirstName]
      ,[LastName]
      ,[DateOfBirth]
      ,[ArmToLegLengthRatio]
      ,[PhoneNumber]
      ,[Salary]
  FROM [Person];
