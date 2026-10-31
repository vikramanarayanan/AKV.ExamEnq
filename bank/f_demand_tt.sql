-- F_DEMAND_TT no longer takes MTHYR. It reads MTHYR and FINE from EAZYPAY_YEAROFEXAM.

CREATE OR REPLACE FUNCTION F_DEMAND_TT
(
   MCOURSE IN VARCHAR2
  ,MREGU IN VARCHAR2
  ,MSEM IN VARCHAR2
  ,MREG IN VARCHAR2
) RETURN NUMBER AS

mdemand NUMBER;
fsem VARCHAR2(2);
mregfees NUMBER;
mprofees NUMBER;
mdegfees NUMBER;
msfees NUMBER;
myr NUMBER;
mpapfees NUMBER;
mfsem NUMBER;
mconsfees NUMBER;
mblind VARCHAR2(20);
mbfees NUMBER;
mfine NUMBER;
mthyr VARCHAR2(3);
BEGIN

mbfees :=0;
mblind := null;
mregfees := 85;
msfees := 90;
myr := 0;
  mprofees := 0;
  mdegfees := 0;
  mconsfees := 0;
  mfine := 0;
  mthyr := NULL;

mpapfees := f_papfees_tt(mcourse,mreg);
mfsem := f_finalsem_tt(mcourse);

BEGIN
  SELECT TRIM(MTHYR), NVL(FINE, 0)
    INTO mthyr, mfine
    FROM EAZYPAY_YEAROFEXAM
   WHERE ROWNUM = 1;
EXCEPTION
  WHEN NO_DATA_FOUND THEN
    mthyr := NULL;
    mfine := 0;
END;

if substr(mthyr,1,1) = 'M' then
  if msem = mfsem then
  mprofees := 550;
  mdegfees := 700;
  if (mcourse >= '300') then
  mconsfees := 1000;
  else
mconsfees := 850;
  end if;

  else
  mprofees := 0;
  mdegfees := 0;
  mconsfees := 0;
  end if;
end if;


 begin
 select fees into mbfees from blind_m26 where regno = mreg;

EXCEPTION
WHEN NO_DATA_FOUND THEN
 mbfees := 0;
 end;



if mbfees > 0 then
mdemand := mbfees + NVL(mfine,0);
else
mdemand  := mregfees+msfees+nvl(mpapfees,0)+mprofees+mdegfees+mconsfees+NVL(mfine,0);
end if;

  RETURN mdemand;
END F_DEMAND_TT;
/

CREATE OR REPLACE VIEW VIEW_API_ROLL AS
select coursecode,f_coursename(coursecode)coursename,collegecode,
f_collegename(collegecode)collegename,photo,case when regno is null then 1 else sem_no end sem_no,case when sem_no = '1' then rollno else regno end regno ,rollno,name,
f_demand_tt(coursecode,regu,sem_no,(case when regno is not null then regno else rollno end)) demand from roll where  papers is not null
/

CREATE OR REPLACE FUNCTION F_DUE
(
 MREGNO IN VARCHAR2
) RETURN VARCHAR2 AS

 mdue NUMBER;
 mdemand NUMBER;

BEGIN
mdue := 0;
mdemand := 0;

 select f_demand_tt(coursecode,regu,sem_no,regno) into mdemand  from roll where regno = mregno;


select (case when mdemand > nvl(paid,0) then mdemand - nvl(paid,0) else 0 end) into mdue from   roll where regno = mregno;


 RETURN mdue;
END F_DUE;
/

CREATE OR REPLACE PROCEDURE P_DEMAND_UPDATE
(
   mreg IN VARCHAR2
 ) AS

BEGIN


 ---FOR m1 IN (select coursecode,regno,rollno,f_demand_Tt(coursecode,regu,sem_no,rollno)demand  from roll where rollno = mreg)
 FOR m1 IN (select coursecode,regno,rollno,f_demand_Tt(coursecode,regu,sem_no,rollno)demand  from roll where coursecode = '104')
 LOOP
  update roll set demand = m1.demand where  rollno = m1.regno;

 END LOOP;
  END P_DEMAND_UPDATE;
/
