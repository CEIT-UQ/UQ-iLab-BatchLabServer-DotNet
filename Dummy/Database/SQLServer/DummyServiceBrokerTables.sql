/********************************************************************************************************************
*/

IF OBJECT_ID (N'[dbo].[Experiments]', 'U' ) IS NOT NULL
BEGIN
    PRINT N'Dropping table [dbo].[Experiments]'
    DROP TABLE [dbo].[Experiments];
END
PRINT N'Creating table [dbo].[Experiments]'
GO
CREATE TABLE [dbo].[Experiments] (
    ExperimentId int IDENTITY (1, 1) NOT NULL,
    LabServerGuid varchar(40) NOT NULL,

    PRIMARY KEY(ExperimentId)
)
GO
