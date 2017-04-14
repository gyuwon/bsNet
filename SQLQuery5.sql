select * from test

CREATE TABLE Test
(
	[Id] INT NOT NULL PRIMARY KEY,
	title nvarchar(4000) not null
)

SELECT *  FROM sys.columns WHERE object_id=OBJECT_ID('test')
SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('test3')
select * from sys.types
 select 
        sep.*
    from sys.tables st
    inner join sys.columns sc on st.object_id = sc.object_id
    left join sys.extended_properties sep on st.object_id = sep.major_id
                                         and sc.column_id = sep.minor_id
                                         
    where st.name = 'test3'

select f1.name, (select top 1 s0.name from sys.types s0 where s0.system_type_id = f1.system_type_id), f1.is_nullable, f1.is_identity, (select top 1 value from sys.extended_properties s0 where f0.object_id = s0.major_id and f1.column_id = s0.minor_id and s0.name = 'MS_Description') comment
from sys.tables f0, sys.columns f1
where f0.object_id = f1.object_id and f0.name = 'test3'
                                         
    
EXEC sys.sp_updateextendedproperty @name=N'b', @value=N'weweewweddd' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'test2', @level2type=N'COLUMN',@level2name=N'title'

GO
EXEC sys.sp_addextendedproperty @name=N'c', @value=N'ddsdsdsd' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'test2', @level2type=N'COLUMN',@level2name=N'title'
GO


select * from sys.extended_properties