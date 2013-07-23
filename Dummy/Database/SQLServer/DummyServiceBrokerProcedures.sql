/********************************************************************************************************************
*/

IF OBJECT_ID (N'[dbo].[Experiments_Add]', 'P' ) IS NOT NULL 
BEGIN
    PRINT N'Dropping procedure [dbo].[Experiments_Add]'
    DROP PROCEDURE [dbo].[Experiments_Add];
END
PRINT N'Creating procedure [dbo].[Experiments_Add]'
GO
CREATE PROCEDURE [dbo].[Experiments_Add]
    @LabServerGuid varchar(40)
AS
BEGIN
    INSERT INTO [dbo].[Experiments] (
        LabServerGuid
    )
    VALUES (
        @LabServerGuid
    )
    SELECT CAST(@@IDENTITY AS int)
    RETURN @@IDENTITY
END
GO

/********************************************************************************************************************
*/

IF OBJECT_ID (N'[dbo].[Experiments_Delete]', 'P' ) IS NOT NULL 
BEGIN
    PRINT N'Dropping procedure [dbo].[Experiments_Delete]'
    DROP PROCEDURE [dbo].[Experiments_Delete];
END
PRINT N'Creating procedure [dbo].[Experiments_Delete]'
GO
CREATE PROCEDURE [dbo].[Experiments_Delete]
    @ExperimentId integer
AS
BEGIN
    DECLARE @Retval integer
	
    DELETE FROM [dbo].[Experiments]
    WHERE ExperimentId = @ExperimentId
    IF @@ERROR <> 0 OR @@ROWCOUNT = 0
        SET @Retval = 0
    ELSE
        SET @Retval = @ExperimentId
		
    SELECT @Retval
    RETURN @Retval
END
GO

/********************************************************************************************************************
*/

IF OBJECT_ID (N'[dbo].[Experiments_GetNextExperimentId]', 'P' ) IS NOT NULL 
BEGIN
    PRINT N'Dropping procedure [dbo].[Experiments_GetNextExperimentId]'
    DROP PROCEDURE [dbo].[Experiments_GetNextExperimentId];
END
PRINT N'Creating procedure [dbo].[Experiments_GetNextExperimentId]'
GO
CREATE PROCEDURE [dbo].[Experiments_GetNextExperimentId]
AS
BEGIN
    DECLARE @Retval integer
	
    IF (SELECT COUNT(*) FROM [dbo].[Experiments]) = 0
        SET @Retval = 1
    ELSE
        SET @Retval = (SELECT (Max(ExperimentId) + 1) FROM [dbo].[Experiments])
		
    SELECT @Retval
    RETURN @Retval
END
GO

/********************************************************************************************************************
*/

IF OBJECT_ID (N'[dbo].[Experiments_RetrieveBy]', 'P' ) IS NOT NULL 
BEGIN
    PRINT N'Dropping procedure [dbo].[Experiments_RetrieveBy]'
    DROP PROCEDURE [dbo].[Experiments_RetrieveBy];
END
PRINT N'Creating procedure [dbo].[Experiments_RetrieveBy]'
GO
CREATE PROCEDURE [dbo].[Experiments_RetrieveBy]
    @ColumnName varchar(16) = NULL,
    @IntValue integer = 0,
    @StrValue varchar(40) = NULL
AS
BEGIN
    IF @ColumnName IS NULL
        SELECT * FROM [dbo].[Experiments]
        ORDER BY ExperimentId ASC
    ELSE IF LOWER(@ColumnName) = 'experimentid'
        SELECT * FROM [dbo].[Experiments]
        WHERE ExperimentId = @IntValue
    ELSE IF LOWER(@ColumnName) = 'labserverguid'
        SELECT * FROM [dbo].[Experiments]
        WHERE LabServerGuid = @StrValue
    RETURN @@ROWCOUNT
END
GO
