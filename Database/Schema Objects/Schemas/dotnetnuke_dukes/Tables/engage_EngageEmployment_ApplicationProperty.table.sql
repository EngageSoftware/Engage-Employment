CREATE TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_ApplicationProperty] (
    [ApplicationPropertyId] INT           IDENTITY (1, 1) NOT NULL,
    [Deleted]               BIT           NOT NULL,
    [DataType]              INT           NOT NULL,
    [DefaultValue]          NTEXT         NULL,
    [PropertyName]          NVARCHAR (50) NOT NULL,
    [Required]              BIT           NOT NULL,
    [ViewOrder]             INT           NOT NULL,
    [Visible]               BIT           NOT NULL,
    [PortalId]              INT           NULL
);

