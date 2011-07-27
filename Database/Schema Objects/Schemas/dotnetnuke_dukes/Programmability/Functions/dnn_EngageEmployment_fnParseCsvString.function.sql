

CREATE FUNCTION dotnetnuke_dukes.dnn_EngageEmployment_fnParseCsvString
(
    @CSVString nvarchar(4000),
    @Delimiter nvarchar(10)
)
RETURNS @tbl table ([Value] nvarchar(1000))
AS
/*
    from http://www.mindsdoor.net/SQLTsql/ParseCSVString.html
    Author Nigel Rivett
*/
BEGIN
DECLARE @i int,
	@j int
	SELECT 	@i = 1
	WHILE @i <= LEN(@CSVString)
	BEGIN
		SELECT	@j = CHARINDEX(@Delimiter, @CSVString, @i)
		IF @j = 0
		BEGIN
			SELECT	@j = LEN(@CSVString) + 1
		END
		INSERT	@tbl SELECT SUBSTRING(@CSVString, @i, @j - @i)
		SELECT	@i = @j + LEN(@Delimiter)
	END
	RETURN
END