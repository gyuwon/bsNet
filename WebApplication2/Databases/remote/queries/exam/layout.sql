#add : 모의상황시험지 추가, exam을 먼저 추가 해야 함
insert into examlayout(exam_rowid,layout_rowid)
select exam_rowid,@layout_rowid:int@ from exam where exam_rowid=@exam_rowid:int@ and examcat_rowid=20

#list : 모의상황 시험지 리스트 
select 
	t0.exam_rowid,t0.examlayout_rowid,t1.teacher_rowid,t2.username teacher_name,t1.examcat_rowid,t1.title,t1.regdate
from examlayout t0
inner join exam t1 on t0.exam_rowid=t1.exam_rowid
left join teacher t2 on t1.teacher_rowid=t2.teacher_rowid
where t1.teacher_rowid=@teacher_rowid:int@
order by t1.exam_rowid desc

#view : 모의상황 시험지 상세 
select 
	t0.exam_rowid,t0.examlayout_rowid,t1.teacher_rowid,t2.username teacher_name,t1.examcat_rowid,t1.title,t1.regdate
from examlayout t0
inner join exam t1 on t0.exam_rowid=t1.exam_rowid
left join teacher t2 on t1.teacher_rowid=t2.teacher_rowid
where t1.exam_rowid=@exam_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@

#sensor_add : 모의상황 시험지 센서 추가 
insert into examlayoutsensor(examlayout_rowid,title)
select examlayout_rowid,@title:examlayoutsensor.title@ from examlayout
where examlayout_rowid=(
	select t0.examlayout_rowid from examlayout t0 inner join exam t1 on t0.exam_rowid=t1.exam_rowid 
	where t0.exam_rowid=@exam_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@
)

#sensor_list : 모의상황 시험지 센서 리스트
select examlayoutsensor_rowid,examlayout_rowid,title from examlayoutsensor where examlayout_rowid=(
	select examlayout_rowid from examlayout where exam_rowid=@exam_rowid:int@
) order by examlayoutsensor_rowid asc

#block_add : 모의상황 시험지 블록 추가 
insert into examlayoutblock(examlayoutsensor_rowid)
select examlayoutsensor_rowid from examlayoutsensor 
where examlayoutsensor_rowid=(
	select t0.examlayoutsensor_rowid from examlayoutsensor t0 
	inner join examlayout t1 on t0.examlayout_rowid=t1.examlayout_rowid
	inner join exam t2 on t1.exam_rowid=t2.exam_rowid 
	where t0.examlayoutsensor_rowid=@examlayoutsensor_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
)

#block_del : 센서의 블록 삭제, 소속 파형이 없어야 하고 마지막 블록은 지우면 안됨 
delete from examlayoutblock where examlayoutblock_rowid=(
	select t0.examlayoutblock_rowid from examlayoutblock t0
	inner join examlayoutsensor t1 on t0.examlayoutsensor_rowid=t1.examlayoutsensor_rowid
	inner join examlayout t2 on t1.examlayout_rowid=t2.examlayout_rowid
	inner join exam t3 on t2.exam_rowid=t3.exam_rowid 
	where t0.examlayoutblock_rowid=@examlayoutblock_rowid:int@ and t3.teacher_rowid=@teacher_rowid:int@
)and 
0=(select count(*)from examlayoutwave where examlayoutblock_rowid=@examlayoutblock_rowid:int@)and -- 파형이 없어야 함
1<(select count(*)from examlayoutblock where examlayoutsensor_rowid=( -- 센서당 1개는 있어야 함
	select t0.examlayoutsensor_rowid from examlayoutblock t0
	inner join examlayoutsensor t1 on t0.examlayoutsensor_rowid=t1.examlayoutsensor_rowid
	inner join examlayout t2 on t1.examlayout_rowid=t2.examlayout_rowid
	inner join exam t3 on t2.exam_rowid=t3.exam_rowid 
	where t0.examlayoutblock_rowid=@examlayoutblock_rowid:int@
))

#block_list : 모의상황 시험지 블록 리스트
select t0.examlayoutblock_rowid,t0.examlayoutsensor_rowid 
from examlayoutblock t0 
inner join examlayoutsensor t1 on t0.examlayoutsensor_rowid=t1.examlayoutsensor_rowid
inner join examlayout t2 on t1.examlayout_rowid=t2.examlayout_rowid
where t2.exam_rowid=@exam_rowid:int@
order by t1.examlayoutsensor_rowid,t0.examlayoutblock_rowid asc

#wave_add : 모의상황 시험지 파형 추가 
insert into examlayoutwave(wave_rowid,examlayoutblock_rowid,offset,lefttrim,righttrim)
select @wave_rowid:int@,examlayoutblock_rowid,@offset:int@,0,0 from examlayoutblock 
where examlayoutblock_rowid=(
	select t0.examlayoutblock_rowid from examlayoutblock t0
	inner join examlayoutsensor t1 on t0.examlayoutsensor_rowid=t1.examlayoutsensor_rowid
	inner join examlayout t2 on t1.examlayout_rowid=t2.examlayout_rowid
	inner join exam t3 on t2.exam_rowid=t3.exam_rowid 
	where t0.examlayoutblock_rowid=@examlayoutblock_rowid:int@ and t3.teacher_rowid=@teacher_rowid:int@
)

#wave_edit : 모의상황 시험지 파형 수정
update examlayoutwave set offset=@offset:int@,lefttrim=@lefttrim:int@,righttrim=@righttrim:int@
where examlayoutwave_rowid=(
	select t0.examlayoutwave_rowid from examlayoutwave t0
	inner join examlayoutblock t1 on t0.examlayoutblock_rowid=t1.examlayoutblock_rowid
	inner join examlayoutsensor t2 on t1.examlayoutsensor_rowid=t2.examlayoutsensor_rowid
	inner join examlayout t3 on t2.examlayout_rowid=t3.examlayout_rowid
	inner join exam t4 on t3.exam_rowid=t4.exam_rowid 
	where t0.examlayoutwave_rowid=@examlayoutwave_rowid:int@ and t4.teacher_rowid=@teacher_rowid:int@
)

#wave_del : 모의상황 시험지 파형 삭제 
delete from examlayoutwave where examlayoutwave_rowid=(
	select t0.examlayoutwave_rowid from examlayoutwave t0
	inner join examlayoutblock t1 on t0.examlayoutblock_rowid=t1.examlayoutblock_rowid
	inner join examlayoutsensor t2 on t1.examlayoutsensor_rowid=t2.examlayoutsensor_rowid
	inner join examlayout t3 on t2.examlayout_rowid=t3.examlayout_rowid
	inner join exam t4 on t3.exam_rowid=t4.exam_rowid 
	where t0.examlayoutwave_rowid=@examlayoutwave_rowid:int@ and t4.teacher_rowid=@teacher_rowid:int@
)

#wave_list1 : 파형 리스트(시험지 전체)
select t0.examlayoutwave_rowid,t0.examlayoutblock_rowid,t0.offset,t0.lefttrim,t0.righttrim 
from examlayoutwave t0 
inner join examlayoutblock t1 on t0.examlayoutblock_rowid=t1.examlayoutblock_rowid
inner join examlayoutsensor t2 on t1.examlayoutsensor_rowid=t2.examlayoutsensor_rowid
inner join examlayout t3 on t2.examlayout_rowid=t3.examlayout_rowid
where t3.exam_rowid=@exam_rowid:int@
order by t2.examlayoutsensor_rowid,t1.examlayoutblock_rowid,t0.offset asc

#wave_list2 : 파형 리스트(블록내)
select examlayoutwave_rowid,examlayoutblock_rowid,offset,lefttrim,righttrim 
from examlayoutwave 
where examlayoutblock_rowid=@examlayoutblock_rowid:int@
order by offset asc


#item_add : 모의시험 객관식 답안지 추가, 답안지는 4개이므로 4번 호출해야 함 
insert into examlayoutitem(examlayout_rowid,title,iscorrect)
select examlayout_rowid,@title:examlayoutitem.title@,@iscorrect:examlayoutitem.iscorrect@ from examlayout
where examlayout_rowid=(
	select t0.examlayout_rowid from examlayout t0 inner join exam t1 on t0.exam_rowid=t1.exam_rowid 
	where t0.exam_rowid=@exam_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@
)

#item_del : 모의시험 객관식 답안지 모두 삭제. 즉, 주관식이 됨.
delete from examlayoutitem where examlayout_rowid=(
	select t0.examlayout_rowid from examlayout t0 inner join exam t1 on t0.exam_rowid=t1.exam_rowid 
	where t0.exam_rowid=@exam_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@
)

#item_list : 모의시험 객관식 답안지 전체 리스트.
select examlayoutitem_rowid,examlayout_rowid,title,iscorrect from examlayoutitem where examlayout_rowid=(
	select examlayout_rowid from examlayout where exam_rowid=@exam_rowid:int@
)
