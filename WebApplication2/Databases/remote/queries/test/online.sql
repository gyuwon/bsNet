#add : 시험상태 추가, 이미 시험중이거나 온라인 상태아 아니면 추가가 무시됨 
insert into onlinetest(online_rowid,test_rowid)
select t1.online_rowid,test_rowid from test t0 inner join online t1 on member_rowid=@member_rowid:int@
where t0.test_rowid=@test_rowid:int@ and state!=3 and 0=(
	select count(*)from onlinetest where online_rowid=t1.online_rowid and test_rowid=t0.test_rowid
)

#view : 시험상태 보기 
select t0.student_rowid,t0.cls_rowid,t1.num,t1.username,t2.online_rowid,t3.onlinetest_rowid
from student t0
left join member t1 on t0.member_rowid=t1.member_rowid
left join online t2 on t0.member_rowid=t2.member_rowid
left join onlinetest t3 on t2.online_rowid=t3.online_rowid and t3.test_rowid=@test_rowid:int@
where t0.member_rowid=@member_rowid:int@

#list : 시험중인 온라인/오프라인 학생 리스트. onlinetest_rowid가 null이면 시험 참가하지 않은 것 임.
select t0.student_rowid,t0.member_rowid,t0.cls_rowid,t1.num,t1.username,t2.online_rowid,t3.onlinetest_rowid
from student t0
left join member t1 on t0.member_rowid=t1.member_rowid
left join online t2 on t0.member_rowid=t2.member_rowid
left join onlinetest t3 on t2.online_rowid=t3.online_rowid and t3.test_rowid=@test_rowid:int@
where t0.cls_rowid=(select cls_rowid from test where test_rowid=@test_rowid:int@)
order by t3.onlinetest_rowid,t1.num asc

#del : 시험에서 학생을 내보냄 
delete from onlinetest where online_rowid=(
	select online_rowid from online where member_rowid=@member_rowid:int@
)

#del_all : 시험 종료되면 학생들 모두 시험에서 내보냄
delete from onlinetest where test_rowid=@test_rowid:int@