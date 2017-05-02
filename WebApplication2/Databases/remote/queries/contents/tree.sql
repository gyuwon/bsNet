#add : 컨텐츠 트리 생성, 적용row수가 0이면 이미 같은 title이 있는 경우임.
insert into contree(parent_rowid,title,ord)
select @parent_rowid:int@,@title:contree.title@,(
	select case when max(ord) is null then 1 else max(ord)+1 end from contree where parent_rowid=@parent_rowid:int@
)from(select 'x'x)a 
where 0=(select count(*)from contree where parent_rowid=@parent_rowid:int@ and title=@title:contree.title@)

#view
select parent_rowid from contree where contree_rowid=@contree_rowid:int@

#list : 컨텐츠 리스트
select contree_rowid,parent_rowid,title,ord,regdate from contree where contree_rowid!=1 order by parent_rowid,ord

#edit : 컨텐츠 트리 수정
update contree set title=@title:contree.title@ where contree_rowid=@contree_rowid:int@ and contree_rowid!=1

#ord : 컨텐츠 트리 순서 바꾸기 
update contree set ord=@ord:int@ where contree_rowid=@contree_rowid:int@ and contree_rowid!=1

#del : 컨텐츠 트리 삭제. 삭제하지 못하면 적용된 row수가 0임. 
delete from contree where contree_rowid=@contree_rowid:int@ and 1=(
	-- 컨텐츠 트리 삭제가 가능한가?(1.컨텐츠가 없어야 하고 2.자식 트리가 없어야 함. 3.그리고 최고 부모 트리는 삭제 못함)
	select 
		case 
			when t0.contree_rowid=1 then 0 -- 최고 부모 트리는 삭제할 수 없음 
			when(select count(*)from contree where parent_rowid=t0.contree_rowid)>0 then 0 -- 자식 트리가 있으면 삭제할 수 없음 
			when(select count(*)from con where contree_rowid=t0.contree_rowid)>0 then 0 -- 트리내에 문서가 있으면 삭제할 수 없음 
			else 1 
		end 
	from contree t0
	where contree_rowid=@contree_rowid:int@ 
)