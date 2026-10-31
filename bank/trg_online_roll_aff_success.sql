-- When ONLINE_ROLL_AFF.BANKSTATUS becomes Success, copy payment
-- fields onto ROLL (matched by ID).

CREATE OR REPLACE TRIGGER TRG_ONLINE_ROLL_AFF_SUCCESS
AFTER UPDATE ON ONLINE_ROLL_AFF
FOR EACH ROW
WHEN (NEW.BANKSTATUS = 'Success')
BEGIN
  IF :NEW.ID IS NOT NULL THEN
    UPDATE ROLL
       SET PAID            = NVL(:NEW.TRANSACTIONAMOUNT, :NEW.PGAMOUNT),
           UNIQUEREFNUMBER = :NEW.UNIQUEREFNUMBER,
           TRANSACTIONDATE = :NEW.TRANSACTIONDATE
     WHERE ID = :NEW.ID;
  END IF;
END;
/
SHOW ERRORS

-- Copy already-successful payments onto ROLL.
UPDATE ROLL r
   SET PAID = (
         SELECT NVL(a.TRANSACTIONAMOUNT, a.PGAMOUNT)
           FROM ONLINE_ROLL_AFF a
          WHERE a.ID = r.ID
            AND a.BANKSTATUS = 'Success'
            AND ROWNUM = 1
       ),
       UNIQUEREFNUMBER = (
         SELECT a.UNIQUEREFNUMBER
           FROM ONLINE_ROLL_AFF a
          WHERE a.ID = r.ID
            AND a.BANKSTATUS = 'Success'
            AND ROWNUM = 1
       ),
       TRANSACTIONDATE = (
         SELECT a.TRANSACTIONDATE
           FROM ONLINE_ROLL_AFF a
          WHERE a.ID = r.ID
            AND a.BANKSTATUS = 'Success'
            AND ROWNUM = 1
       )
 WHERE EXISTS (
         SELECT 1
           FROM ONLINE_ROLL_AFF a
          WHERE a.ID = r.ID
            AND a.BANKSTATUS = 'Success'
       );

COMMIT;
EXIT;
