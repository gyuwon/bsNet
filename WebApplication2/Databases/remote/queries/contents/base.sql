#cat_list
select concat_rowid,upfilecat_rowid,title from concat

#add : 컨텐츠 추가, upfile과 concat을 검증하는 로직이 필요할 수 있다. 가령 해당 upfile이 concat소속인지 확인할 필요있을 수 있음 
insert into con(contree_rowid,concat_rowid,upfile_rowid,title,ord)values(
	@contree_rowid:int@,@concat_rowid:int@,@upfile_rowid:int@,@title:con.title@,(
		select case when max(ord) is null then 1 else max(ord)+1 end from con where contree_rowid=@contree_rowid:int@
	)
)

#list : 트리에 속한 컨텐츠 리스트
select 
	t0.con_rowid,t0.title,t0.ord,t0.regdate,
	t1.concat_rowid,t1.title catname,
	t2.upfile_rowid,t2.originname upfilename,concat(t4.basepath,t2.upfile)upfilepath
from con t0
left join concat t1 on t0.concat_rowid=t1.concat_rowid
left join upfile t2 on t0.upfile_rowid=t2.upfile_rowid
left join upfilecatext t3 on t2.upfilecatext_rowid=t3.upfilecatext_rowid
left join upfilecat t4 on t3.upfilecat_rowid=t4.upfilecat_rowid
where t0.contree_rowid=@contree_rowid:int@ order by t0.ord asc

#view : 컨텐츠 상세
select 
	t0.con_rowid,t0.title,t0.ord,t0.regdate,
	t1.concat_rowid,t1.title catname,
	t2.upfile_rowid,t2.originname upfilename,concat(t4.basepath,t2.upfile)upfilepath
from con t0
left join concat t1 on t0.concat_rowid=t1.concat_rowid
left join upfile t2 on t0.upfile_rowid=t2.upfile_rowid
left join upfilecatext t3 on t2.upfilecatext_rowid=t3.upfilecatext_rowid
left join upfilecat t4 on t3.upfilecat_rowid=t4.upfilecat_rowid
where t0.con_rowid=@con_rowid:int@

#edit : 수정, upfile과 concat을 검증하는 로직이 필요할 수 있다. 가령 해당 upfile이 concat소속인지 확인할 필요있을 수 있음 
update con set title=@title:con.title@,@concat_rowid:int@,upfile_rowid=@upfile_rowid:int@ where con_rowid=@con_rowid:int@

#ord : 순서바꾸기
update con set ord=@ord:int@ where con_rowid=@con_rowid:int@

#del : 삭제, upfile도 함께 삭제해야 할 수 있다.  
delete from con where con_rowid=@con_rowid:int@





