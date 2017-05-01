#add : 클래스 생성 
insert into cls(teacher_rowid,title)values(@teacher_rowid:int@,@title:cls.title@)

#view : 클래스 기본정보
select cls_rowid,teacher_rowid,title,regdate from cls where cls_rowid=@cls_rowid:int@

#list_for_teacher : 교수입장 클래스 리스트
select 
	t0.cls_rowid,t0.teacher_rowid,t0.title,t0.regdate,
	(select count(*)from student where cls_rowid=t0.cls_rowid)studentcnt,
	(
		select count(*)from test f0 inner join exam f1 on f0.exam_rowid=f1.exam_rowid and f1.examcat_rowid=10
		where f0.cls_rowid=t0.cls_rowid and f0.state=3
	)pdcnt, -- PD파형 시험본 수(시험대기나 시험중인 것은 제외)
	(
		select count(*)from test f0 inner join exam f1 on f0.exam_rowid=f1.exam_rowid and f1.examcat_rowid=20
		where f0.cls_rowid=t0.cls_rowid and f0.state=3
	)layoutcnt, -- 모의상황 시험본 수 (시험대기나 시험중인 것은 제외)
	(
		select count(*)from test f0 inner join exam f1 on f0.exam_rowid=f1.exam_rowid and f1.examcat_rowid=30
		where f0.cls_rowid=t0.cls_rowid and f0.state=3
	)device1cnt, -- 설비 온라인 시험 본 수 (시험대기나 시험중인 것은 제외)
	(
		select count(*)from test f0 inner join exam f1 on f0.exam_rowid=f1.exam_rowid and f1.examcat_rowid=40
		where f0.cls_rowid=t0.cls_rowid and f0.state=3
	)device2cnt -- 설비현장시험 본 수 (시험대기나 시험중인 것은 제외)	
from cls t0
where t0.teacher_rowid=@teacher_rowid:int@
order by t0.cls_rowid desc

#list_for_student : 학생입장 소속 클래스 리스트 
select 
	t0.cls_rowid,t0.teacher_rowid,t0.title,t0.regdate,
	t1.teacher_rowid,t1.username teacher_name
from cls t0
left join teacher t1 on t0.teacher_rowid=t1.teacher_rowid
where t0.cls_rowid in(select cls_rowid from student where member_rowid=@member_rowid:int@)
order by t0.cls_rowid desc

#edit : 클래스 수정 - 교수용 
update cls set title=@title:cls.title@ where cls_rowid=@cls_rowid:int@ and teacher_rowid=@teacher_rowid:int@

#del : 클래스 삭제 - 교수용, 관련된 table도 삭제되어야 함 
delete from cls where where cls_rowid=@cls_rowid:int@ and teacher_rowid=@teacher_rowid:int@
