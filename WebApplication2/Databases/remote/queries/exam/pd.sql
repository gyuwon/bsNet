#add : PD파형 시험지 추가, exam을 먼저 추가 해야 함
insert into exampd(exam_rowid)
select exam_rowid from exam where exam_rowid=@exam_rowid:int@ and examcat_rowid=10

#list : PD 파형 시험지 리스트. item_cnt이나 total_minute이 0이면 답안지가 없는 경우라 시험 실시 할 수 없음 
select 
	t0.exam_rowid,t1.teacher_rowid,t2.username teacher_name,t1.examcat_rowid,t1.title,t1.regdate,
	(select case when sum(repeatcnt) is null then 0 else sum(repeatcnt) end from exampditem where exampd_rowid=t0.exampd_rowid)total_minute,
	(select count(*)from exampditem where exampd_rowid=t0.exampd_rowid)item_cnt
from exampd t0
inner join exam t1 on t0.exam_rowid=t1.exam_rowid
left join teacher t2 on t1.teacher_rowid=t2.teacher_rowid
where t1.teacher_rowid=@teacher_rowid:int@
order by t1.exam_rowid desc

#view : PD 파형 시험지 정보. item_cnt이나 total_minute이 0이면 답안지가 없는 경우라 시험 실시 할 수 없음 
select 
	t0.exam_rowid,t1.teacher_rowid,t2.username teacher_name,t1.examcat_rowid,t1.title,t1.regdate,
	(select case when sum(repeatcnt) is null then 0 else sum(repeatcnt) end from exampditem where exampd_rowid=t0.exampd_rowid)total_minute,
	(select count(*)from exampditem where exampd_rowid=t0.exampd_rowid)item_cnt
from exampd t0
inner join exam t1 on t0.exam_rowid=t1.exam_rowid
left join teacher t2 on t1.teacher_rowid=t2.teacher_rowid
where t1.exam_rowid=@exam_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@

#item_add : PD파형 답안지 추가 
insert into exampditem(exampd_rowid,wave_rowid,repeatcnt)
select exampd_rowid,@wave_rowid:int@,@repeatcnt:int@ from exampd 
where exampd_rowid=(
	select t0.exampd_rowid from exampd t0 inner join exam t1 on t0.exam_rowid=t1.exam_rowid 
	where t0.exam_rowid=@exam_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@
)

#item_list : PD파형 답안지 리스트 
select exampditem_rowid,exampd_rowid,wave_rowid,repeatcnt,regdate 
from exampditem where exampd_rowid=(
	select t0.exampd_rowid from exampd t0 inner join exam t1 on t0.exam_rowid=t1.exam_rowid 
	where t0.exam_rowid=@exam_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@
)

#item_edit : PD파형 답안지 수정
update exampditem set wave_rowid=@wave_rowid:int@,repeatcnt=@repeatcnt:int@ 
where exampditem_rowid=(
	select t0.exampditem_rowid from exampditem t0 
	inner join exampd t1 on t0.exampd_rowid=t1.exampd_rowid
	inner join exam t2 on t1.exam_rowid=t2.exam_rowid 
	where t0.exampditem_rowid=@exampditem_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
)
