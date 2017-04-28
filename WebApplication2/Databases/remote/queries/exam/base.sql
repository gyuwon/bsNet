#add : 시험지 추가, 반드시 pd,모의상황,설비 시험 중 하나를 함께 추가해야 함.
insert into exam(teacher_rowid,examcat_rowid,title)values(@teacher_rowid:int@,@examcat_rowid:int@,@title:exam.title@)

#view : 시험지 정보 보기 
select exam_rowid,teacher_rowid,examcat_rowid,title,regdate from exam where exam_rowid=@exam_rowid:int@ and teacher_rowid=@teacher_rowid:int@

#edit : 시험지 수정(제목만)
update exam set title=@title:exam.title@ where exam_rowid=@exam_rowid:int@ and teacher_rowid=@teacher_rowid:int@
