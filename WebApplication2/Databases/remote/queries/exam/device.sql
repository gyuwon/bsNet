#add : 설비시험지 추가, exam을 먼저 추가 해야 함
insert into examdevice(exam_rowid,layout_rowid)
select exam_rowid,@layout_rowid:int@ from exam where exam_rowid=@exam_rowid:int@ and examcat_rowid in(30,40)


#list : 설비(온라인/현장) 시험지 리스트 
select 
	t0.exam_rowid,t0.examdevice_rowid,t1.teacher_rowid,t2.username teacher_name,t1.examcat_rowid,t1.title,t1.regdate
from examdevice t0
inner join exam t1 on t0.exam_rowid=t1.exam_rowid
left join teacher t2 on t1.teacher_rowid=t2.teacher_rowid
where t1.teacher_rowid=@teacher_rowid:int@
order by t1.exam_rowid desc

#view : 설비(온라인/현장) 시험지 상세 
select 
	t0.exam_rowid,t0.examdevice_rowid,t1.teacher_rowid,t2.username teacher_name,t1.examcat_rowid,t1.title,t1.regdate
from examdevice t0
inner join exam t1 on t0.exam_rowid=t1.exam_rowid
left join teacher t2 on t1.teacher_rowid=t2.teacher_rowid
where t1.exam_rowid=@exam_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@

#sensor_add
insert into examdevicesensor(examdevice_rowid,title)
select examdevice_rowid,@title:examdevicesensor.title@ from examdevice
where examdevice_rowid=(
	select t0.examdevice_rowid from examdevice t0 inner join exam t1 on t0.exam_rowid=t1.exam_rowid 
	where t0.exam_rowid=@exam_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@
)

#sensor_list 
select examdevicesensor_rowid,examdevice_rowid,title from examdevicesensor where examdevice_rowid=(
	select examdevice_rowid from examdevice where exam_rowid=@exam_rowid:int@
) order by examdevicesensor_rowid asc

#block_add
insert into examdeviceblock(examdevicesensor_rowid)
select examdevicesensor_rowid from examdevicesensor 
where examdevicesensor_rowid=(
	select t0.examdevicesensor_rowid from examdevicesensor t0 
	inner join examdevice t1 on t0.examdevice_rowid=t1.examdevice_rowid
	inner join exam t2 on t1.exam_rowid=t2.exam_rowid 
	where t0.examdevicesensor_rowid=@examdevicesensor_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
)

#block_del : 센서의 블록 삭제, 소속 파형이 없어야 하고 마지막 블록은 지우면 안됨 
delete from examdeviceblock where examdeviceblock_rowid=(
	select t0.examdeviceblock_rowid from examdeviceblock t0
	inner join examdevicesensor t1 on t0.examdevicesensor_rowid=t1.examdevicesensor_rowid
	inner join examdevice t2 on t1.examdevice_rowid=t2.examdevice_rowid
	inner join exam t3 on t2.exam_rowid=t3.exam_rowid 
	where t0.examdeviceblock_rowid=@examdeviceblock_rowid:int@ and t3.teacher_rowid=@teacher_rowid:int@
)and 
0=(select count(*)from examdevicewave where examdeviceblock_rowid=@examdeviceblock_rowid:int@)and -- 파형이 없어야 함
1<(select count(*)from examdeviceblock where examdevicesensor_rowid=( -- 센서당 1개는 있어야 함
	select t0.examdevicesensor_rowid from examdeviceblock t0
	inner join examdevicesensor t1 on t0.examdevicesensor_rowid=t1.examdevicesensor_rowid
	inner join examdevice t2 on t1.examdevice_rowid=t2.examdevice_rowid
	inner join exam t3 on t2.exam_rowid=t3.exam_rowid 
	where t0.examdeviceblock_rowid=@examdeviceblock_rowid:int@
))

#block_list 
select t0.examdeviceblock_rowid,t0.examdevicesensor_rowid 
from examdeviceblock t0 
inner join examdevicesensor t1 on t0.examdevicesensor_rowid=t1.examdevicesensor_rowid
inner join examdevice t2 on t1.examdevice_rowid=t2.examdevice_rowid
where t2.exam_rowid=@exam_rowid:int@
order by t1.examdevicesensor_rowid,t0.examdeviceblock_rowid asc

#wave_add
insert into examdevicewave(wave_rowid,examdeviceblock_rowid,offset,lefttrim,righttrim)
select @wave_rowid:int@,examdeviceblock_rowid,@offset:int@,0,0 from examdeviceblock 
where examdeviceblock_rowid=(
	select t0.examdeviceblock_rowid from examdeviceblock t0
	inner join examdevicesensor t1 on t0.examdevicesensor_rowid=t1.examdevicesensor_rowid
	inner join examdevice t2 on t1.examdevice_rowid=t2.examdevice_rowid
	inner join exam t3 on t2.exam_rowid=t3.exam_rowid 
	where t0.examdeviceblock_rowid=@examdeviceblock_rowid:int@ and t3.teacher_rowid=@teacher_rowid:int@
)

#wave_edit
update examdevicewave set offset=@offset:int@,lefttrim=@lefttrim:int@,righttrim=@righttrim:int@
where examdevicewave_rowid=(
	select t0.examdevicewave_rowid from examdevicewave t0
	inner join examdeviceblock t1 on t0.examdeviceblock_rowid=t1.examdeviceblock_rowid
	inner join examdevicesensor t2 on t1.examdevicesensor_rowid=t2.examdevicesensor_rowid
	inner join examdevice t3 on t2.examdevice_rowid=t3.examdevice_rowid
	inner join exam t4 on t3.exam_rowid=t4.exam_rowid 
	where t0.examdevicewave_rowid=@examdevicewave_rowid:int@ and t4.teacher_rowid=@teacher_rowid:int@
)

#wave_del
delete from examdevicewave where examdevicewave_rowid=(
	select t0.examdevicewave_rowid from examdevicewave t0
	inner join examdeviceblock t1 on t0.examdeviceblock_rowid=t1.examdeviceblock_rowid
	inner join examdevicesensor t2 on t1.examdevicesensor_rowid=t2.examdevicesensor_rowid
	inner join examdevice t3 on t2.examdevice_rowid=t3.examdevice_rowid
	inner join exam t4 on t3.exam_rowid=t4.exam_rowid 
	where t0.examdevicewave_rowid=@examdevicewave_rowid:int@ and t4.teacher_rowid=@teacher_rowid:int@
)

#wave_list1 : 시험지에 속한 모든 파형 들 
select t0.examdevicewave_rowid,t0.examdeviceblock_rowid,t0.offset,t0.lefttrim,t0.righttrim 
from examdevicewave t0 
inner join examdeviceblock t1 on t0.examdeviceblock_rowid=t1.examdeviceblock_rowid
inner join examdevicesensor t2 on t1.examdevicesensor_rowid=t2.examdevicesensor_rowid
inner join examdevice t3 on t2.examdevice_rowid=t3.examdevice_rowid
where t3.exam_rowid=@exam_rowid:int@
order by t2.examdevicesensor_rowid,t1.examdeviceblock_rowid,t0.offset asc

#wave_list2 : 하나의 블록에 있는 파형들 
select examdevicewave_rowid,examdeviceblock_rowid,offset,lefttrim,righttrim 
from examdevicewave 
where examdeviceblock_rowid=@examdeviceblock_rowid:int@
order by offset asc


#item_cat_list : 시험지 항목 종류 리스트
select examdeviceitemcat_rowid,title from examdeviceitemcat

#item_add : 시험지 항목 추가
insert into examdeviceitem(examdevice_rowid,examdeviceitemcat_rowid,contents,ord)
select 
	examdevice_rowid,@examdeviceitemcat_rowid:int@,@contents:examdeviceitem.contents@,
 	(select case when max(ord) is null then 1 else max(ord)+1 end from examdeviceitem where examdevice_rowid=t0.examdevice_rowid)
from examdevice t0 
where exam_rowid=@exam_rowid:int@

#item_list1 : 시험지 항목 리스트
select 
	t0.examdeviceitem_rowid,t0.examdevice_rowid,t0.examdeviceitemcat_rowid,t0.contents,t0.ord,t0.regdate,
	t1.examdeviceitemupfile_rowid,t2.upfile_rowid,t2.originname upfilename,concat(t4.basepath,t2.upfile)upfilepath
from examdeviceitem t0
left join examdeviceitemupfile t1 on t0.examdeviceitem_rowid=t1.examdeviceitem_rowid
left join upfile t2 on t1.upfile_rowid=t2.upfile_rowid
left join upfilecatext t3 on t2.upfilecatext_rowid=t3.upfilecatext_rowid
left join upfilecat t4 on t3.upfilecat_rowid=t4.upfilecat_rowid
inner join examdevice t5 on t0.examdevice_rowid=t5.examdevice_rowid
where t5.exam_rowid=@exam_rowid:int@
order by t0.ord asc

#item_list2 : 시험지 객관식 항목 중 답안을 골라야 하는 항목만 가져오기 
select 
	t0.examdeviceitem_rowid,t0.examdevice_rowid,t0.examdeviceitemcat_rowid,t0.contents,t0.ord,t0.regdate,
	t1.examdeviceitemupfile_rowid,t2.upfile_rowid,t2.originname upfilename,concat(t4.basepath,t2.upfile)upfilepath
from examdeviceitem t0
left join examdeviceitemupfile t1 on t0.examdeviceitem_rowid=t1.examdeviceitem_rowid
left join upfile t2 on t1.upfile_rowid=t2.upfile_rowid
left join upfilecatext t3 on t2.upfilecatext_rowid=t3.upfilecatext_rowid
left join upfilecat t4 on t3.upfilecat_rowid=t4.upfilecat_rowid
inner join examdevice t5 on t0.examdevice_rowid=t5.examdevice_rowid
where 
	t5.exam_rowid=@exam_rowid:int@ and 
	t0.examdeviceitemcat_rowid=20 and 
	0=(select count(*)from examdeviceitem20 where examdeviceitem_rowid=t0.examdeviceitem_rowid and iscorrect=1)
order by t0.ord asc

#item_view : 시험지 항목 상세
select 
	t0.examdeviceitem_rowid,t0.examdevice_rowid,t0.examdeviceitemcat_rowid,t0.contents,t0.ord,t0.regdate,
	t2.upfile_rowid,t2.originname upfilename,concat(t4.basepath,t2.upfile)upfilepath
from examdeviceitem t0
left join examdeviceitemupfile t1 on t0.examdeviceitem_rowid=t1.examdeviceitem_rowid
left join upfile t2 on t1.upfile_rowid=t2.upfile_rowid
left join upfilecatext t3 on t2.upfilecatext_rowid=t3.upfilecatext_rowid
left join upfilecat t4 on t3.upfilecat_rowid=t4.upfilecat_rowid
where t0.examdeviceitem_rowid=@examdeviceitem_rowid:int@

#item_edit : 시험지 항목 수정
update examdeviceitem set contents=@contents:examdeviceitem.contents@
where examdeviceitem_rowid=(
	select examdeviceitem_rowid from examdeviceitem t0
	inner join examdevice t1 on t0.examdevice_rowid=t1.examdevice_rowid
	inner join exam t2 on t1.exam_rowid=t2.exam_rowid
	where t0.examdeviceitem_rowid=@examdeviceitem_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
)

#item_del : 시험지 항목 삭제
delete from examdeviceitem where examdeviceitem_rowid=(
	select examdeviceitem_rowid from examdeviceitem t0
	inner join examdevice t1 on t0.examdevice_rowid=t1.examdevice_rowid
	inner join exam t2 on t1.exam_rowid=t2.exam_rowid
	where t0.examdeviceitem_rowid=@examdeviceitem_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
)

#item_ord : 시험지 항목 순번 수정 
update examdeviceitem set ord=@ord:int@ 
where examdeviceitem_rowid=(
	select examdeviceitem_rowid from examdeviceitem t0
	inner join examdevice t1 on t0.examdevice_rowid=t1.examdevice_rowid
	inner join exam t2 on t1.exam_rowid=t2.exam_rowid
	where t0.examdeviceitem_rowid=@examdeviceitem_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
)

#item_upfile_add : 시험지 항목 이미지 추가
insert into examdeviceitemupfile(examdeviceitem_rowid,upfile_rowid)
select examdeviceitem_rowid,@upfile_rowid:int@ from examdeviceitem where examdeviceitem_rowid=(
	select examdeviceitem_rowid from examdeviceitem t0
	inner join examdevice t1 on t0.examdevice_rowid=t1.examdevice_rowid
	inner join exam t2 on t1.exam_rowid=t2.exam_rowid
	where t0.examdeviceitem_rowid=@examdeviceitem_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
)

#item_upfile_del : 시험지 항목 이미지 삭제 
delete from examdeviceitemupfile where examdeviceitem_rowid=(
	select examdeviceitem_rowid from examdeviceitem t0
	inner join examdevice t1 on t0.examdevice_rowid=t1.examdevice_rowid
	inner join exam t2 on t1.exam_rowid=t2.exam_rowid
	where t0.examdeviceitem_rowid=@examdeviceitem_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
)


#item20_add : 설비(온라인/현장) 객관식 답안지 추가, 답안지는 4개이므로 4번 호출해야 함 
insert into examdeviceitem20(examdeviceitem_rowid,title,iscorrect)
select examdeviceitem_rowid,@title:examdeviceitem20.title@,@iscorrect:examdeviceitem20.iscorrect@ from examdeviceitem
where examdeviceitem_rowid=(
	select examdeviceitem_rowid from examdeviceitem t0
	inner join examdevice t1 on t0.examdevice_rowid=t1.examdevice_rowid
	inner join exam t2 on t1.exam_rowid=t2.exam_rowid
	where t0.examdeviceitem_rowid=@examdeviceitem_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@ and t0.examdeviceitemcat_rowid=20
)

#item20_del : 설비(온라인/현장) 객관식 답안지 모두 삭제. 적용된 결과는 4개가 나와야 함 
delete from examdeviceitem20 where examdeviceitem_rowid=(
	select examdeviceitem_rowid from examdeviceitem t0
	inner join examdevice t1 on t0.examdevice_rowid=t1.examdevice_rowid
	inner join exam t2 on t1.exam_rowid=t2.exam_rowid
	where t0.examdeviceitem_rowid=@examdeviceitem_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@
)

#item20_list1 : 설비(온라인/현장) 객관식 답안지 리스트 - 전체를 가져온다. 
select 
	t0.examdeviceitem20_rowid,t0.examdeviceitem_rowid,t0.title,t0.iscorrect,
	t1.examdeviceitem20upfile_rowid,t2.upfile_rowid,t2.originname upfilename,concat(t4.basepath,t2.upfile)upfilepath
from examdeviceitem20 t0
left join examdeviceitem20upfile t1 on t0.examdeviceitem20_rowid=t1.examdeviceitem20_rowid
left join upfile t2 on t1.upfile_rowid=t2.upfile_rowid
left join upfilecatext t3 on t2.upfilecatext_rowid=t3.upfilecatext_rowid
left join upfilecat t4 on t3.upfilecat_rowid=t4.upfilecat_rowid
inner join examdeviceitem t5 on t0.examdeviceitem_rowid=t5.examdeviceitem_rowid
inner join examdevice t6 on t5.examdevice_rowid=t6.examdevice_rowid
inner join exam t7 on t6.exam_rowid=t7.exam_rowid
where t7.exam_rowid=@exam_rowid:int@
order by t5.ord,examdeviceitem20_rowid asc

#item20_list2 : 설비(온라인/현장) 객관식 답안지 리스트 - 하나의 객관식에 대해서만 가져온다.
select 
	t0.examdeviceitem20_rowid,t0.examdeviceitem_rowid,t0.title,t0.iscorrect,
	t1.examdeviceitem20upfile_rowid,t2.upfile_rowid,t2.originname upfilename,concat(t4.basepath,t2.upfile)upfilepath
from examdeviceitem20 t0
left join examdeviceitem20upfile t1 on t0.examdeviceitem20_rowid=t1.examdeviceitem20_rowid
left join upfile t2 on t1.upfile_rowid=t2.upfile_rowid
left join upfilecatext t3 on t2.upfilecatext_rowid=t3.upfilecatext_rowid
left join upfilecat t4 on t3.upfilecat_rowid=t4.upfilecat_rowid
inner join examdeviceitem t5 on t0.examdeviceitem_rowid=t5.examdeviceitem_rowid
inner join examdevice t6 on t5.examdevice_rowid=t6.examdevice_rowid
inner join exam t7 on t6.exam_rowid=t7.exam_rowid
where t5.examdeviceitem_rowid=@examdeviceitem_rowid:int@
order by examdeviceitem20_rowid asc

#item20_edit : 설비(온라인/현장) 객관식 답안지 수정
update examdeviceitem20 set title=@title:examdeviceitem20.title@,iscorrect=@iscorrect:examdeviceitem20.iscorrect@
where examdeviceitem20_rowid=(
	select examdeviceitem20_rowid from examdeviceitem20 t0
	inner join examdeviceitem t1 on t0.examdeviceitem_rowid=t1.examdeviceitem_rowid
	inner join examdevice t2 on t1.examdevice_rowid=t2.examdevice_rowid
	inner join exam t3 on t2.exam_rowid=t3.exam_rowid
	where t0.examdeviceitem20_rowid=@examdeviceitem20_rowid:int@ and t3.teacher_rowid=@teacher_rowid:int@ and t1.examdeviceitemcat_rowid=20
)

#item20_upfile_add : 시험지 객관식 답안 이미지 추가
insert into examdeviceitem20upfile(examdeviceitem20_rowid,upfile_rowid)
select examdeviceitem20_rowid,@upfile_rowid:int@ from examdeviceitem20 where examdeviceitem20_rowid=(
	select examdeviceitem20_rowid from examdeviceitem20 t0
	inner join examdeviceitem t1 on t0.examdeviceitem_rowid=t1.examdeviceitem_rowid
	inner join examdevice t2 on t1.examdevice_rowid=t2.examdevice_rowid
	inner join exam t3 on t2.exam_rowid=t3.exam_rowid
	where t0.examdeviceitem20_rowid=@examdeviceitem20_rowid:int@ and t3.teacher_rowid=@teacher_rowid:int@ and t1.examdeviceitemcat_rowid=20
)

#item20_upfile_del : 시험지 객관식 답안  이미지 삭제
delete from examdeviceitem20upfile where examdeviceitem20_rowid=(
	select examdeviceitem20_rowid from examdeviceitem20 t0
	inner join examdeviceitem t1 on t0.examdeviceitem_rowid=t1.examdeviceitem_rowid
	inner join examdevice t2 on t1.examdevice_rowid=t2.examdevice_rowid
	inner join exam t3 on t2.exam_rowid=t3.exam_rowid
	where t0.examdeviceitem20_rowid=@examdeviceitem20_rowid:int@ and t3.teacher_rowid=@teacher_rowid:int@ and t1.examdeviceitemcat_rowid=20
)


