
#교수 추가 
insert into teacher(cmps_rowid,username)values(@cmps_rowid:int@,@username:teacher.username@)
insert into teacher(cmps_rowid,username)values(1,'나교수');

#사원 추가 
insert into member(num,username,pw)values(@num:member.num@,@username:member.username@,@pw:member.pw@)

#클래스 추가  
insert into cls(teacher_rowid,title)values(@teacher_rowid:int@,@title:cls.title@)
insert into cls(teacher_rowid,title)values(1,'1703-A')

# 교수가 담당하는 클래스 시험결과 리스트
select 
	t0.cls_rowid cls_r,t0.title,convert(varchar(10),t0.regdate,120)regdate,
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
	)device2cnt, -- 설비현장시험 본 수 (시험대기나 시험중인 것은 제외)
	t1.regdate pdstartdate,
	t1.score pdscore,
	t2.regdate layoutstartdate,
	t2.success layoutsuccess
from cls t0
left join(
	-- 특정교수의 가장 최근에 완료한 클래스별 PD파형 시험의 날짜와 평균점수(10점만점기준)
	select 
		j1.exam_rowid,j1.test_rowid,j1.cls_rowid,
		convert(varchar(10),j1.regdate,120)regdate,
		format(j1.correctcnt * 10.0 / (j1.membercnt * j1.itemcnt * 1.0), '#0.0')score 
	from(
		select 
			f2.exam_rowid,f0.test_rowid,f0.cls_rowid,f0.regdate,
			(
				select count(*)from(
					select member_rowid from answerpd where testpditem_rowid in(select testpditem_rowid from testpditem where testpd_rowid=f1.testpd_rowid)group by member_rowid
				)a
			)membercnt, -- 답을 한번이라도 제출한 회원수  
			(
				select count(*)from testpditem where testpd_rowid=f1.testpd_rowid
			)itemcnt,  -- 문제수 
			(
				select count(*)from answerpd i0 
				left join testpditem i1 on i0.testpditem_rowid=i1.testpditem_rowid 
				where i0.testpditem_rowid in(select testpditem_rowid from testpditem where testpd_rowid=f1.testpd_rowid)and i0.wave_rowid=i1.wave_rowid
			)correctcnt -- 정답수(정답율 = correctcnt / (membercnt * itemcnt))
		from test f0 
		inner join testpd f1 on f0.test_rowid=f1.test_rowid
		inner join exam f2 on f0.exam_rowid=f2.exam_rowid 
		where f0.test_rowid in( -- 특정 교수의 클래스별 최근 완료한 시험들만 추출 
			select max(f0.test_rowid)test_rowid
			from test f0
			inner join testpd f1 on f0.test_rowid=f1.test_rowid
			inner join exam f2 on f0.exam_rowid=f2.exam_rowid
			where f2.teacher_rowid=@teacher_rowid:int@ and f0.state=3 group by f0.cls_rowid 
		)
	)j1
)t1 on t0.cls_rowid=t1.cls_rowid
left join(
	-- 특정교수의 가장 최근에 완료한 클래스별 모의상황 시험의 날짜와 성공/실패 여부 
	select 
		j2.cls_rowid,j2.regdate,
		case when (j2.correctcnt * 1.0 / j2.membercnt * 1.0) < 0.5 then '실패' else '성공' end success
	from(
		select 
			a.exam_rowid,a.test_rowid,a.cls_rowid,a.testlayout_rowid,a.hasitem,
			convert(varchar(10),a.regdate,120)regdate,
			-- 정답을 맞춘 회원수 
			case when hasitem = 1 then 
				--객관식의 경우 정답을 맞춘 회원수 
				(	
					select count(*)from(
						select * from(
							select 
								d.member_rowid,
								count(*)correctcnt, -- 정답인 항목수 
								(
									select count(*)from answerlayout1 where 
									member_rowid=d.member_rowid and 
									testlayoutitem_rowid in(select testlayoutitem_rowid from testlayoutitem where testlayout_rowid=a.testlayout_rowid)
								)answercnt -- 맞다고 제출한 항목수 
							from answerlayout1 d 
							where 
								testlayoutitem_rowid in(select testlayoutitem_rowid from testlayoutitem where testlayout_rowid=a.testlayout_rowid and iscorrect=1) 
							group by d.member_rowid
						)c where correctcnt=(select count(*) from testlayoutitem where testlayout_rowid=a.testlayout_rowid and iscorrect=1)
					)b where correctcnt=answercnt -- 정답수와 제출수가 일치해야 함 
				)
			else
				--주관식의 경우 정답을 맞춘 회원수  
				(select count(*)from answerlayout2 where testlayout_rowid=a.testlayout_rowid and marking=1)
			end correctcnt, 
	
			-- 답을 한번이라도 제출한 회원수  
			case when hasitem = 1 then 
				(select count(*)from(
					select member_rowid from answerlayout1 where testlayoutitem_rowid in(select testlayoutitem_rowid from testlayoutitem where testlayout_rowid=a.testlayout_rowid) group by member_rowid
				)b)
			else
				(select count(*)from answerlayout2 where testlayout_rowid=a.testlayout_rowid)
			end membercnt 
		from(
			select 
				f2.exam_rowid,f0.test_rowid,f0.cls_rowid,f1.testlayout_rowid,f0.regdate,
				case when 0=(select count(*)from testlayoutitem where testlayout_rowid=f1.testlayout_rowid)then 0 else 1 end hasitem --객관식인가?
			from test f0
			inner join testlayout f1 on f0.test_rowid=f1.test_rowid
			inner join exam f2 on f0.exam_rowid=f2.exam_rowid
			where f0.test_rowid in( -- 특정 교수의 클래스별 최근 완료한 시험들만 추출 
				select max(f0.test_rowid)test_rowid
				from test f0
				inner join testlayout f1 on f0.test_rowid=f1.test_rowid
				inner join exam f2 on f0.exam_rowid=f2.exam_rowid
				where f2.teacher_rowid=@teacher_rowid:int@ and f0.state=3 group by f0.cls_rowid 
			)
		)a
	)j2 
)t2 on t0.cls_rowid=t2.cls_rowid
where t0.teacher_rowid=@teacher_rowid:int@;
