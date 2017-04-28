#add : 모의상황시험 생성, test생성후 호출할 수 있음. 만약 추가시 적용row가 0 이면 에러처리 할 것
insert into testlayout(test_rowid,layout_rowid)
select t0.test_rowid,@layout_rowid:int@ from test t0
inner join exam t1 on t0.exam_rowid=t1.exam_rowid
inner join examlayout t2 on t1.exam_rowid=t2.exam_rowid
where t0.test_rowid=@test_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@ and t1.examcat_rowid=20 and
0<(
	select count(*)from examlayoutwave w0 
	inner join examlayoutblock w1 on w0.examlayoutblock_rowid=w1.examlayoutblock_rowid
	inner join examlayoutsensor w2 on w1.examlayoutsensor_rowid=w2.examlayoutsensor_rowid
	where w2.examlayout_rowid=t2.examlayout_rowid
) -- 센서파형이 1개 이상 있어야 함.

#view : 모의상황 시험 정보 보기 
select 
	t0.test_rowid,t0.testlayout_rowid,t2.exam_rowid,t2.teacher_rowid,t3.username teachername,t2.examcat_rowid,t1.title,t1.regdate
from testlayout t0
inner join test t1 on t0.test_rowid=t1.test_rowid
inner join exam t2 on t1.exam_rowid=t2.exam_rowid
left join teacher t3 on t2.teacher_rowid=t3.teacher_rowid
where t0.test_rowid=@test_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@


#ready_for_add : 추가할 모의상황 시험 센서,블록,파형 가져오기 
select 
	t3.exam_rowid,t3.examlayout_rowid,t2.examlayoutsensor_rowid,t1.examlayoutblock_rowid,t0.examlayoutwave_rowid,
	t4.teacher_rowid,t4.examcat_rowid,t4.title exam_title,t3.layout_rowid,t2.title sensor_title,t0.wave_rowid,t0.offset,t0.lefttrim,t0.righttrim
from examlayoutwave t0
right join examlayoutblock t1 on t0.examlayoutblock_rowid=t1.examlayoutblock_rowid   -- 파형이 없는 블록이 있을 수 있으므로 right join
inner join examlayoutsensor t2 on t1.examlayoutsensor_rowid=t2.examlayoutsensor_rowid  -- 블록이 없는 센서는 없다고 본다. 그래서 inner join함 
inner join examlayout t3 on t2.examlayout_rowid=t3.examlayout_rowid
inner join exam t4 on t3.exam_rowid=t4.exam_rowid
where t4.exam_rowid=@exam_rowid:int@ and t4.examcat_rowid=20
order by t2.examlayoutsensor_rowid,t1.examlayoutblock_rowid,t0.offset asc -- 센서,블록 단위로 정렬 

#senser_add: 모의상황 시험 센서 추가.
insert into testlayoutsensor(testlayout_rowid,title)
select @testlayout_rowid:int@,title from examlayoutsensor where examlayoutsensor_rowid=@examlayoutsensor_rowid:int@

#sensor_list
select testlayoutsensor_rowid,testlayout_rowid,title from testlayoutsensor where testlayout_rowid=(
	select testlayout_rowid from testlayout where test_rowid=@test_rowid:int@
) order by testlayoutsensor_rowid asc

#block_add : 모의상황 시험 센서 블록 추가. 
insert into testlayoutblock(testlayoutsensor_rowid)
select @testlayoutsensor_rowid:int@ from examlayoutblock where examlayoutblock_rowid=@examlayoutblock_rowid:int@

#block_list
select t0.testlayoutblock_rowid,t0.testlayoutsensor_rowid 
from testlayoutblock t0 
inner join testlayoutsensor t1 on t0.testlayoutsensor_rowid=t1.testlayoutsensor_rowid
inner join testlayout t2 on t1.testlayout_rowid=t2.testlayout_rowid
where t2.test_rowid=@test_rowid:int@
order by t1.testlayoutsensor_rowid,t0.testlayoutblock_rowid asc

#wave_add : 모의상황 시험 센서 파형 추가
insert into testlayoutwave(wave_rowid,testlayoutblock_rowid,offset,lefttrim,righttrim)
select wave_rowid,@testlayoutblock_rowid:int@,offset,lefttrim,righttrim from examlayoutwave where examlayoutwave_rowid=@examlayoutwave_rowid:int@

#wave_list
select t0.testlayoutwave_rowid,t0.testlayoutblock_rowid,t0.offset,t0.lefttrim,t0.righttrim 
from testlayoutwave t0 
inner join testlayoutblock t1 on t0.testlayoutblock_rowid=t1.testlayoutblock_rowid
inner join testlayoutsensor t2 on t1.testlayoutsensor_rowid=t2.testlayoutsensor_rowid
inner join testlayout t3 on t2.testlayout_rowid=t3.testlayout_rowid
where t3.test_rowid=@test_rowid:int@
order by t2.testlayoutsensor_rowid,t1.testlayoutblock_rowid,t0.offset asc

#item_add : 모의상황 시험 답안(객관식) 추가
insert into testlayoutitem(testlayout_rowid,title,iscorrect)
select t4.testlayout_rowid,t0.title,t0.iscorrect 
from examlayoutitem t0
inner join examlayout t1 on t0.examlayout_rowid=t1.examlayout_rowid
inner join exam t2 on t1.exam_rowid=t2.exam_rowid
inner join test t3 on t2.exam_rowid=t3.exam_rowid
inner join testlayout t4 on t3.test_rowid=t4.test_rowid
where t3.test_rowid=@test_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
order by t0.examlayoutitem_rowid asc


#item_list : 모의시험 객관식 답안 전체 리스트.
select testlayoutitem_rowid,testlayout_rowid,title,iscorrect from testlayoutitem where testlayout_rowid=(
	select testlayout_rowid from testlayout where test_rowid=@test_rowid:int@
)

