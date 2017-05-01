#add : 설비시험 생성, test생성후 호출할 수 있음. 만약 추가시 적용row가 0 이면 에러처리 할 것
insert into testdevice(test_rowid,layout_rowid)
select t0.test_rowid,@layout_rowid:int@ from test t0
inner join exam t1 on t0.exam_rowid=t1.exam_rowid
inner join examdevice t2 on t1.exam_rowid=t2.exam_rowid
where t0.test_rowid=@test_rowid:int@ and t1.teacher_rowid=@teacher_rowid:int@ and t1.examcat_rowid in(30,40)and
0<(
	select count(*)from examdevicewave w0 
	inner join examdeviceblock w1 on w0.examdeviceblock_rowid=w1.examdeviceblock_rowid
	inner join examdevicesensor w2 on w1.examdevicesensor_rowid=w2.examdevicesensor_rowid
	where w2.examdevice_rowid=t2.examdevice_rowid
) -- 센서파형이 1개 이상 있어야 함.

#view : 설비시험 정보 보기 
select 
	t0.test_rowid,t0.testdevice_rowid,t2.exam_rowid,t2.teacher_rowid,t3.username teachername,t2.examcat_rowid,t1.title,t1.regdate
from testdevice t0
inner join test t1 on t0.test_rowid=t1.test_rowid
inner join exam t2 on t1.exam_rowid=t2.exam_rowid
left join teacher t3 on t2.teacher_rowid=t3.teacher_rowid
where t0.test_rowid=@test_rowid:int@ and t2.teacher_rowid=@teacher_rowid:int@

#add_ready : 추가할 설비(온라인/현장) 시험 센서,블록,파형 가져오기 
select 
	t3.exam_rowid,t3.examdevice_rowid,t2.examdevicesensor_rowid,t1.examdeviceblock_rowid,t0.examdevicewave_rowid,
	t4.teacher_rowid,t4.examcat_rowid,t4.title exam_title,t3.layout_rowid,t2.title sensor_title,t0.wave_rowid,t0.offset,t0.lefttrim,t0.righttrim
from examdevicewave t0
right join examdeviceblock t1 on t0.examdeviceblock_rowid=t1.examdeviceblock_rowid   -- 파형이 없는 블록이 있을 수 있으므로 right join
inner join examdevicesensor t2 on t1.examdevicesensor_rowid=t2.examdevicesensor_rowid  -- 블록이 없는 센서는 없다고 본다. 그래서 inner join함 
inner join examdevice t3 on t2.examdevice_rowid=t3.examdevice_rowid
inner join exam t4 on t3.exam_rowid=t4.exam_rowid
where t4.exam_rowid=@exam_rowid:int@ and t4.examcat_rowid in(30,40)
order by t2.examdevicesensor_rowid,t1.examdeviceblock_rowid,t0.offset asc -- 센서,블록 단위로 정렬 

#senser_add: 설비(온라인/현장) 시험 센서 추가.
insert into testdevicesensor(testdevice_rowid,title)
select @testdevice_rowid:int@,title from examdevicesensor where examdevicesensor_rowid=@examdevicesensor_rowid:int@

#sensor_list 
select testdevicesensor_rowid,testdevice_rowid,title from testdevicesensor where testdevice_rowid=(
	select testdevice_rowid from testdevice where test_rowid=@test_rowid:int@
) order by testdevicesensor_rowid asc

#block_add : 설비(온라인/현장) 시험 센서 블록 추가. 
insert into testdeviceblock(testdevicesensor_rowid)
select @testdevicesensor_rowid:int@ from examdeviceblock where examdeviceblock_rowid=@examdeviceblock_rowid:int@

#block_list 
select t0.testdeviceblock_rowid,t0.testdevicesensor_rowid 
from testdeviceblock t0 
inner join testdevicesensor t1 on t0.testdevicesensor_rowid=t1.testdevicesensor_rowid
inner join testdevice t2 on t1.testdevice_rowid=t2.testdevice_rowid
where t2.test_rowid=@test_rowid:int@
order by t1.testdevicesensor_rowid,t0.testdeviceblock_rowid asc

#wave_add : 설비(온라인/현장) 시험 센서 파형 추가
insert into testdevicewave(wave_rowid,testdeviceblock_rowid,offset,lefttrim,righttrim)
select wave_rowid,@testdeviceblock_rowid:int@,offset,lefttrim,righttrim from examdevicewave where examdevicewave_rowid=@examdevicewave_rowid:int@

#wave_list
select t0.testdevicewave_rowid,t0.testdeviceblock_rowid,t0.offset,t0.lefttrim,t0.righttrim 
from testdevicewave t0 
inner join testdeviceblock t1 on t0.testdeviceblock_rowid=t1.testdeviceblock_rowid
inner join testdevicesensor t2 on t1.testdevicesensor_rowid=t2.testdevicesensor_rowid
inner join testdevice t3 on t2.testdevice_rowid=t3.testdevice_rowid
where t3.test_rowid=@test_rowid:int@
order by t2.testdevicesensor_rowid,t1.testdeviceblock_rowid,t0.offset asc

#item_add : 설비 시험 항목 추가 
insert into testdeviceitem(testdevice_rowid,examdeviceitemcat_rowid,contents)
select @testdevice_rowid:int@,examdeviceitemcat_rowid,contents from examdeviceitem where examdeviceitem_rowid=@examdeviceitem_rowid:int@

#item_upfile_add : 설비 시험 항목 업파일 추가 
insert into testdeviceitemupfile(testdeviceitem_rowid,upfile_rowid)
select @testdeviceitem_rowid:int@,upfile_rowid from examdeviceitemupfile where examdeviceitemupfile_rowid=@examdeviceitemupfile_rowid:int@

#item_list : 설비 시험 항목 리스트
select 
	t0.testdeviceitem_rowid,t0.testdevice_rowid,t0.examdeviceitemcat_rowid,t0.contents,
	t1.testdeviceitemupfile_rowid,t2.upfile_rowid,t2.originname upfilename,concat(t4.basepath,t2.upfile)upfilepath
from testdeviceitem t0
left join testdeviceitemupfile t1 on t0.testdeviceitem_rowid=t1.testdeviceitem_rowid
left join upfile t2 on t1.upfile_rowid=t2.upfile_rowid
left join upfilecatext t3 on t2.upfilecatext_rowid=t3.upfilecatext_rowid
left join upfilecat t4 on t3.upfilecat_rowid=t4.upfilecat_rowid
inner join testdevice t5 on t0.testdevice_rowid=t5.testdevice_rowid
where t5.test_rowid=@test_rowid:int@
order by t0.testdeviceitem_rowid asc


#item20_add : 설비 시험 객관식 답안지 추가 
insert into testdeviceitem20(testdeviceitem_rowid,title,iscorrect)
select @testdeviceitem_rowid:int@,title,iscorrect from examdeviceitem20 where examdeviceitem20_rowid=@examdeviceitem20_rowid:int@

#item20_upfile_add : 설비 시험 객관식 답안지 업파일 추가 
insert into testdeviceitem20upfile(testdeviceitem20_rowid,upfile_rowid)
select @testdeviceitem20_rowid:int@,upfile_rowid from examdeviceitem20upfile where examdeviceitem20upfile_rowid=@examdeviceitem20upfile_rowid:int@

#item20_edit : 설비 시험 객관식 답안지 정답 수정
update testdeviceitem20 set iscorrect=@iscorrect:int@ where testdeviceitem20_rowid=@testdeviceitem20_rowid:int@

#item20_list : 설비(온라인/현장) 객관식 답안 리스트 - 전체를 가져온다. 
select 
	t0.testdeviceitem20_rowid,t0.testdeviceitem_rowid,t0.title,t0.iscorrect,
	t1.testdeviceitem20upfile_rowid,t2.upfile_rowid,t2.originname upfilename,concat(t4.basepath,t2.upfile)upfilepath
from testdeviceitem20 t0
left join testdeviceitem20upfile t1 on t0.testdeviceitem20_rowid=t1.testdeviceitem20_rowid
left join upfile t2 on t1.upfile_rowid=t2.upfile_rowid
left join upfilecatext t3 on t2.upfilecatext_rowid=t3.upfilecatext_rowid
left join upfilecat t4 on t3.upfilecat_rowid=t4.upfilecat_rowid
inner join testdeviceitem t5 on t0.testdeviceitem_rowid=t5.testdeviceitem_rowid
inner join testdevice t6 on t5.testdevice_rowid=t6.testdevice_rowid
inner join test t7 on t6.test_rowid=t7.test_rowid
where t7.test_rowid=@test_rowid:int@
order by t5.testdeviceitem_rowid,testdeviceitem20_rowid asc

