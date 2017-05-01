#add : 교수 추가  
insert into teacher(cmps_rowid,username)values(@cmps_rowid:int@,@username:teacher.username@)

#view_from_cmps_rowid : 교수정보 보기 
select teacher_rowid,cmps_rowid,username,regdate from teacher where cmps_rowid=@cmps_rowid:int@

#view_from_rowid : 교수정보 보기 
select teacher_rowid,cmps_rowid,username,regdate from teacher where teacher_rowid=@teacher_rowid:int@

#del : 교수삭제 
delete from teacher where teacher_rowid=@teacher_rowid:int@

#edit : 교수정보 수정 
update teacher set username=@username:teacher.username@ where teacher_rowid=@teacher_rowid:int@
