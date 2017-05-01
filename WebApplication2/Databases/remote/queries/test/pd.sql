#add : PD시험 생성, test생성후 호출할 수 있음. 만약 추가시 적용row가 0 이면 에러처리 할 것
insert into testpd(test_rowid)
select t0.test_rowid from test t0
inner join exam t1 on t0.exam_rowid=t1.exam_rowid
inner join exampd t2 on t1.exam_rowid=t2.exam_rowid
where t0.test_rowid=@test_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@ and t1.examcat_rowid=10 and
0<(select count(*)from exampditem where exampd_rowid=t2.exampd_rowid) -- 답안지가 1개 이상 있어야 함

#view : PD 파형 시험 정보
select 
	t0.testpd_rowid,t1.test_rowid,t2.exam_rowid,t3.teacher_rowid,t3.username teacher_name,t2.examcat_rowid,t1.title,t1.regdate,t1.state,
	(select case when sum(repeatcnt) is null then 0 else sum(repeatcnt) end from testpditem where testpd_rowid=t0.testpd_rowid)total_minute,
	(select count(*)from testpditem where testpd_rowid=t0.testpd_rowid)item_cnt
from testpd t0
inner join test t1 on t0.test_rowid=t1.test_rowid
inner join exam t2 on t1.exam_rowid=t2.exam_rowid
left join teacher t3 on t2.teacher_rowid=t3.teacher_rowid
where t1.test_rowid=@test_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@

#item_add : PD시험 답안 생성, 현 시험지의 답안지에서 복사함 
insert into testpditem(testpd_rowid,wave_rowid,repeatcnt,startdate,enddate)
select t4.testpd_rowid,t0.wave_rowid,t0.repeatcnt,null,null 
from exampditem t0
inner join exampd t1 on t0.exampd_rowid=t1.exampd_rowid
inner join exam t2 on t1.exam_rowid=t2.exam_rowid
inner join test t3 on t2.exam_rowid=t3.exam_rowid
inner join testpd t4 on t3.test_rowid=t4.test_rowid
where t3.test_rowid=@test_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
order by t0.exampditem_rowid asc

#item_list : PD시험 답안 리스트
select testpditem_rowid,testpd_rowid,wave_rowid,repeatcnt,startdate,enddate  
from testpditem where testpd_rowid=(
	select w0.testpd_rowid from testpd w0 inner join test w1 on w0.test_rowid=w1.test_rowid 
	where w0.test_rowid=@test_rowid:int@ 
)

#item_is_all_end : PD시험 답안들이 모두 종료되었는가?
select case when 0=count(*) then 1 else 0 end from testpditem where testpd_rowid=(
	select w0.testpd_rowid from testpd w0 inner join test w1 on w0.test_rowid=w1.test_rowid 
	where w0.test_rowid=@test_rowid:int@ 
)and(startdate is null or enddate is null)

#item_start : PD시험 답안 시작, 적용수가 1이어야 시작이 된 것임. 
update testpditem set startdate=getdate() 
where testpditem_rowid=(
	select top(1) testpditem_rowid from testpditem t0
	where testpd_rowid=(
		select w0.testpd_rowid from testpd w0 inner join test w1 on w0.test_rowid=w1.test_rowid 
		where w0.test_rowid=@test_rowid:int@ and w1.state=2
	)and 
	startdate is null and -- startdate가 null인 가장 첫번째 답안을 선택 
	0=( -- 답안들중 startdate와 null이 아닌데, enddate가 null인 경우는 안됨.
		select count(*)from testpditem where testpd_rowid=t0.testpd_rowid and startdate is not null and enddate is null
	)
	order by testpditem_rowid asc
)

#item_end : PD시험 답안 종료, 적용수가 1이어야만 종료가 된 것임 
update testpditem set enddate=getdate()
where testpditem_rowid=(
	select top(1) testpditem_rowid from testpditem t0
	where testpd_rowid=(
		select w0.testpd_rowid from testpd w0 inner join test w1 on w0.test_rowid=w1.test_rowid 
		where w0.test_rowid=@test_rowid:int@ and w1.state=2 
	)and startdate is not null and enddate is null and getdate()>=dateadd(mi, repeatcnt, startdate)
)

#item_pass : PD시험 답안 패스, 적용수가 1이어야만 패스된 것임
update testpditem set enddate=getdate()
where testpditem_rowid=(
	select top(1) testpditem_rowid from testpditem 
	where testpd_rowid=(
		select w0.testpd_rowid from testpd w0 inner join test w1 on w0.test_rowid=w1.test_rowid 
		where w0.test_rowid=@test_rowid:int@ and w1.state=2 
	)and startdate is not null and enddate is null
)

#item_pass_all1 : PD시험 답안 종료1, 교수가 시험을 중단시킬 때 요청한다.
update testpditem set startdate=getdate()
where testpd_rowid=(
	select w0.testpd_rowid from testpd w0 inner join test w1 on w0.test_rowid=w1.test_rowid 
	where w0.test_rowid=@test_rowid:int@ and w1.state=2 
)and startdate is null

#item_pass_all2 : PD시험 답안 종료2, 교수가 시험을 중단시킬 때 요청한다.
update testpditem set enddate=getdate()
where testpd_rowid=(
	select w0.testpd_rowid from testpd w0 inner join test w1 on w0.test_rowid=w1.test_rowid 
	where w0.test_rowid=@test_rowid:int@ and w1.state=2 
)and enddate is null

#answer_add : 학생 응답 추가, 응답성공하면 적용수가 1임.
insert into answerpd(member_rowid,testpditem_rowid,wave_rowid)
select @member_rowid:int@,testpditem_rowid,@wave_rowid:int@ from testpditem 
where testpditem_rowid=(
	select t0.testpditem_rowid from testpditem t0
	inner join testpd t1 on t0.testpd_rowid=t1.testpd_rowid 
	inner join test t2 on t1.test_rowid=t2.test_rowid
	inner join student t3 on t2.cls_rowid=t3.cls_rowid and t3.member_rowid=@member_rowid:int@
	where testpditem_rowid=@testpditem_rowid:int@ and t2.state=2
)and 
startdate is not null and enddate is null and -- 제출할 수 있는 답안임을 확인 
0=(select count(*)from answerpd where testpditem_rowid=@testpditem_rowid:int@ and member_rowid=@member_rowid:int@) -- 같은 답안에 제출을 2번 하지 못하도록 함 

#answer_cnt_list : 학생 답안 별 정답/오답 수, 교수 PD 시험 화면에서 정답/오답수 표시 시 이용할 것
select 
	(select count(*)from answerpd where testpditem_rowid=t0.testpditem_rowid and wave_rowid=t0.wave_rowid)correct_cnt, -- 정답수
	(select count(*)from answerpd where testpditem_rowid=t0.testpditem_rowid and wave_rowid!=t0.wave_rowid)incorrect_cnt, -- 오답수 
	(select count(*)from student where cls_rowid=t2.cls_rowid)total_cnt -- 전체학생수 
from testpditem t0
left join testpd t1 on t0.testpd_rowid=t1.testpd_rowid
left join test t2 on t1.test_rowid=t2.test_rowid
where t0.testpd_rowid=(
	select w0.testpd_rowid from testpd w0 inner join test w1 on w0.test_rowid=w1.test_rowid 
	where w0.test_rowid=@test_rowid:int@
)

#answer_list_in_testing : (시험중,교수용)학생별 답안 제출 정보 리스트 
select 
	t0.student_rowid,t0.member_rowid,t0.cls_rowid,t1.num,t1.username,t2.online_rowid,t3.onlinetest_rowid,
	(
		select count(*) from answerpd s0 
		inner join testpditem s1 on s0.testpditem_rowid=s1.testpditem_rowid
		inner join testpd s2 on s1.testpd_rowid=s2.testpd_rowid
		where s2.test_rowid=@test_rowid:int@ and member_rowid=t0.member_rowid and s0.wave_rowid=s1.wave_rowid
	)correct_cnt, -- 정답수 
	t4.testpditem_rowid, -- 진행중인 답안 일련번호 
	t4.wave_rowid correct_wave_rowid, -- 정답 답안
	t5.wave_rowid answer_wave_rowid, -- 제출 답안 
	t4.startdate startdate, -- 답안 시작날짜
	t5.regdate answerdate -- 답안 제출 날짜 
from student t0
left join member t1 on t0.member_rowid=t1.member_rowid
left join online t2 on t0.member_rowid=t2.member_rowid
left join onlinetest t3 on t2.online_rowid=t3.online_rowid and t3.test_rowid=@test_rowid:int@
left join( -- 진행중인 답안
	select 
		top(1) j0.testpditem_rowid,j0.wave_rowid,j0.startdate
	from testpditem j0 
	inner join testpd j1 on j0.testpd_rowid=j1.testpd_rowid
	where j1.test_rowid=@test_rowid:int@ and j0.startdate is not null and j0.enddate is null
)t4 on 1=1
left join answerpd t5 on t1.member_rowid=t5.member_rowid and t4.testpditem_rowid=t5.testpditem_rowid
where t0.cls_rowid=(select cls_rowid from test where test_rowid=@test_rowid:int@)
order by t3.onlinetest_rowid,t1.num asc

#answer_list_for_student : (시험중, 학생용)
select 
	t0.testpditem_rowid,t0.testpd_rowid,t0.repeatcnt,
	t0.wave_rowid correct_wave_rowid, -- 정답 답안
	t1.wave_rowid answer_wave_rowid, -- 제출 답안 
	t0.startdate, -- 답안 시작날짜
	t1.regdate answerdate, -- 답안 제출 날짜 
	t0.enddate  -- 답안 종료날짜 
from testpditem t0
left join answerpd t1 on t0.testpditem_rowid=t1.testpditem_rowid and member_rowid=@member_rowid:int@
where t0.testpd_rowid=(
	select w0.testpd_rowid from testpd w0 inner join test w1 on w0.test_rowid=w1.test_rowid 
	where w0.test_rowid=@test_rowid:int@
)