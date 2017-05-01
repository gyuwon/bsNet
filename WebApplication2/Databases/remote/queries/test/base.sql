#is_testing : 교수기준으로 시험을 하나라도 치루는 중인가?
select case when count(*)>0 then 1 else 0 end from test t0 
inner join exam t1 on t0.exam_rowid=t1.exam_rowid 
where t1.teacher_rowid=@teacher_rowid:int@ and t0.state!=3

-- 시험을 시작할 수 있는 있는 조건 
-- 1. 클래스에 학생이 있을때
-- 2. 어떤 시험도 치루지 않고 있을때 
-- 3. 기타 해당 시험 조건에 맞을때(가령, pd의 경우는 답안지가 1개 이상 있어야 함 )

#add : 시험생성
insert into test(exam_rowid,cls_rowid,title,state)
select exam_rowid,@cls_rowid:int@,title,1 from exam t0
where t0.exam_rowid=@exam_rowid:int@ and 
	t0.teacher_rowid=@teacher_rowid:int@ and 
	1=(select count(*)from cls where cls_rowid=@cls_rowid:int@ and teacher_rowid=t0.teacher_rowid)and
	0<(select count(*)from student where cls_rowid=@cls_rowid:int@)and -- 클래스에 학생이 있음을 확인 
	0=( -- 어떤 시험도 치루고 있지 않음을 확인  
		select count(*)from test w0 
		inner join exam w1 on w0.exam_rowid=w1.exam_rowid 
		where w1.teacher_rowid=t0.teacher_rowid and w0.state!=3
	)

#view : 시험정보
select test_rowid,exam_rowid,cls_rowid,title,state,regdate from test where test_rowid=@test_rowid:int@

#start : 시험시작
update test set state=2 where test_rowid=@test_rowid:int@ and state=1

#end : 시험종료
update test set state=3 where test_rowid=@test_rowid:int@ and state=2

#list_in_progressing : 학생입장에서 시험 중인 시험 리스트 
select
	t0.test_rowid,t0.exam_rowid,t0.cls_rowid,t0.title,t0.state,t0.regdate,
	t1.teacher_rowid,t2.username teacher_name,
	t1.examcat_rowid,t4.title examcat_name,
	t3.title cls_name
from test t0
left join exam t1 on t0.exam_rowid=t1.exam_rowid
left join teacher t2 on t1.teacher_rowid=t2.teacher_rowid
left join cls t3 on t0.cls_rowid=t3.cls_rowid
left join examcat t4 on t1.examcat_rowid=t4.examcat_rowid
where t0.cls_rowid in(
	select cls_rowid from student where member_rowid=@member_rowid:int@
)and state!=3
order by t0.test_rowid asc