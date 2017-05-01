#cnt : 클래스내 학생수. 시험시작전에 클래스 학생수를 체크하는데 사용하면 됨 
select count(*)from student where cls_rowid=@cls_rowid:int@

#list_for_add : 클래스에 등록가능한 학생후보 리스트
select member_rowid,num,username,regdate from member 
where member_rowid not in(select member_rowid from student where cls_rowid=@cls_rowid:int@)

#add : 클래스에 학생 등록
insert into student(cls_rowid,member_rowid)
select @cls_rowid:int@,@member_rowid:int@ from(select 'x'x)a 
where 0=(select count(*)from student where cls_rowid=@cls_rowid:int@ and member_rowid=@member_rowid:int@)

#del : 클래스에서 학생 삭제(교수전용)
delete from student 
where student_rowid=@student_rowid:int@ and 1=(
	select count(*)from cls where cls_rowid=(
		select cls_rowid from student where student_rowid=@student_rowid:int@
	)and teacher_rowid=@teacher_rowid:int@
)
