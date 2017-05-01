#add : 사원 로그인시 추가한다.
insert into online(member_rowid)values(@member_rowid:int@)

#edit : 사원 로그인 상태를 유지할 때 쓴다.
update online set regdate=getdate() where member_rowid=@member_rowid:int@

#view : 사원 로그인 상태인지 확인한다. 
select online_rowid,member_rowid,regdate from online where member_rowid=@member_rowid:int@

#del : 사원 회원탈퇴를 하거나 로그인이 풀릴 때 삭제한다. 
delete from online where member_rowid=@member_rowid:int@