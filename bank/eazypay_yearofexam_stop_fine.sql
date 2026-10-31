-- Show-payment flag and late fine on the current exam-year row.
-- SHOWPAYMENT: 1 = Proceed Now is shown, 0 = portal stopped.
-- FINE: NUMBER(4), added into F_DEMAND_TT demand when > 0.

ALTER TABLE EAZYPAY_YEAROFEXAM ADD SHOWPAYMENT NUMBER(1) DEFAULT 1 NOT NULL;
ALTER TABLE EAZYPAY_YEAROFEXAM ADD FINE NUMBER(4) DEFAULT 0 NOT NULL;

UPDATE EAZYPAY_YEAROFEXAM SET SHOWPAYMENT = 1, FINE = NVL(FINE, 0);
COMMIT;

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
