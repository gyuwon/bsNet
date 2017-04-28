#add : 사원 추가(중복된 사번은 무시함)
insert into member(num,username,pw)
select @num:member.num@,@username:member.username@,@pw:member.pw@ from(select 'x'x)a 
where 0=(select count(*)from member where num=@num:member.num@)

#login : 사원 로그인
select member_rowid,num,username,regdate from member where num=@num:string@ and pw=@pw:string@

#view : 사원 기본정보
select member_rowid,num,username,regdate from member where member_rowid=@member_rowid:int@

#list : 전체리스트 
select member_rowid,num,username,regdate from member order by num asc

#pw_edit
update member set pw=@pw:member.pw@ where member_rowid=@member_rowid:int@