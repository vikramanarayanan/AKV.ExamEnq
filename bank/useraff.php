<?php 
error_reporting(E_ALL);
require_once 'dbconfig.php';
class USER
{

	private $conn;

	public function __construct()
	{
		$database = new Database();
		$db = $database->dbConnection();
		$this->conn = $db;
    }

	public function runQuery($sql)
	{
		$stmt = $this->conn->prepare($sql);
		return $stmt;
	}
 


        function getbankuid() {

        $stmt =  $this->conn->prepare("DELETE from `coeannam_account`.`bankuid`;INSERT into `coeannam_account`.`bankuid` (value) values (1);");
        $stmt->execute();
        $stmt->closeCursor();
        $stmt =  $this->conn->prepare("SELECT id from `coeannam_account`.`bankuid`;");
        $stmt->execute();
        $row = $stmt->fetch(PDO::FETCH_ASSOC);
        return $row['id'];
     //   return generateNumericOTP(8);

    }
       


 

 


    function eazypay_aes128Encrypt($str,$key){
        $block = mcrypt_get_block_size('rijndael_128', 'ecb');
        $pad = $block - (strlen($str) % $block);
        $str .= str_repeat(chr($pad), $pad);
        return base64_encode(mcrypt_encrypt(MCRYPT_RIJNDAEL_128, $key, $str, MCRYPT_MODE_ECB));

    }
 
    function roll_log_insert($refno,$submerchantid,$pgamount,$stname,$email,$mob,$regno,$rollno,$uid,$fcode,$coursecode,$semester,$mode,$transactionamount,$uniquerefnumber,$transactiondate,$bankstatus,$papers,$faculty,$token,$provfees,$degfees,$confees,$coursename,$collegecode,$collegename,$fine)
    {
        try
        {
            $data = [
                       'refno' => $refno,
                       'submerchantid' => $submerchantid,
                       'pgamount' => $pgamount,
                       'stname' => $stname,
                       'email' => $email,
                       'mobileno' => $mob,
                       'regno' => $regno,
                       'rollno' => $rollno,
                       'uid' => $uid,
                       'fcode' => $fcode,
                       'coursecode' => $coursecode,
                       'semester'=>$semester,
                       'mode' => $mode,
                       'transactionamount' => $transactionamount,
                       'uniquerefnumber' => $uniquerefnumber,
                       'transactiondate' => $transactiondate,
                       'bankstatus' => $bankstatus,
                       'papers' => $papers,
                       'faculty' => $faculty,
                       'token' => $token,
                       'provfees' => $provfees,
                       'degfees' => $degfees,
                       'confees' => $confees,
                       'coursename' => $coursename,
                       'collegecode' => $collegecode,
                       'collegename' =>$collegename,
                       'fine' =>$fine
                       
                   ];
        

            $stmt =  $this->conn->prepare("INSERT INTO   `coeannam_account`.`online_roll_aff` (refno,submerchantid,pgamount,stname,email,mobileno,regno,rollno,uid,fcode,
semester,coursecode,mode,transactionamount,uniquerefnumber,transactiondate,bankstatus,papers,faculty,token,provfees,degfees,confees,coursename,collegecode,collegename,fine)
			                                             VALUES(:refno,:submerchantid,:pgamount,:stname,:email,:mobileno,:regno,:rollno,:uid,:fcode,
:semester,:coursecode,:mode,:transactionamount,:uniquerefnumber,:transactiondate,:bankstatus,:papers,:faculty,:token,:provfees,:degfees,:confees,:coursename,:collegecode,:collegename,:fine);
INSERT INTO  `coeannam_account`.`bank_return_log_aff` (refno,submerchantid,pgamount,stname,email,mobileno,regno,rollno,uid,fcode,
semester,coursecode,mode,transactionamount,uniquerefnumber,transactiondate,bankstatus,papers,faculty,token,provfees,degfees,confees,coursename,collegecode,collegename,fine)
			                                             VALUES(:refno,:submerchantid,:pgamount,:stname,:email,:mobileno,:regno,:rollno,:uid,:fcode,
:semester,:coursecode,:mode,:transactionamount,:uniquerefnumber,:transactiondate,:bankstatus,:papers,:faculty,:token,:provfees,:degfees,:confees,:coursename,:collegecode,:collegename,:fine);");

            $stmt->execute($data);

            return $stmt;
        }
        catch(PDOException $ex)
        {
            echo $ex->getMessage();
        }

    }

    function bank_status_update($refno,$transactionamount,$uniquerefnumber,$transactiondate,$bankstatus)
    {
        try
        {
            $data = [
                       'refno' => $refno,
                       'transactionamount' => $transactionamount,
                       'uniquerefnumber' => $uniquerefnumber,
                       'transactiondate' => $transactiondate,
                       'bankstatus' => $bankstatus,

                   ];
           
            $stmt =  $this->conn->prepare("update  `coeannam_account`.`online_roll_aff` set transactionamount=:transactionamount,uniquerefnumber=:uniquerefnumber,transactiondate=:transactiondate,bankstatus=:bankstatus where refno=:refno;
                                           update  `coeannam_account`.`bank_return_log_aff` set transactionamount=:transactionamount,uniquerefnumber=:uniquerefnumber,transactiondate=:transactiondate,bankstatus=:bankstatus where refno=:refno;
");
			    $stmt->execute($data);

                return $stmt;
        }
        catch(PDOException $ex)
        {
            echo $ex->getMessage();
        }

    }

 public function posttobankregular($submerchantid,$pgamount,$stname,$email,$mob,$regno,$rollno,$uid,$fcode,$semester,$coursecode,$mode,$papers,$faculty,$token,$provfees,$degfees,$confees,$coursename,$collegecode,$collegename,$fine)
    {
       

$aes_key='1162121111101159';
$refno = $this->getbankuid();
$eazypay_Payment_url='https://eazypay.icicibank.com/EazyPG';
$mob = '9999999999';
$email='test@gmail.com';

$params = array('mandatory fields'=>$refno.'|'.$submerchantid.'|'.$pgamount.'|'.$stname.'|'.$regno.'|'.$uid.'|'.$fcode.'|'.$mob.'|'.$email,
                'optional fields'=>'',
                'returnurl'=>'https://aucoeexam.in/bank/returnurl',
                'Reference No' =>$refno,
                'submerchantid'=>$submerchantid,
                'transaction amount'=>$pgamount,
                'paymode'=> '9',);
$getparam1 = $getparam = '?merchantid=380738';

foreach($params as $keys => $values)
{
   // if(false)
    if ($keys != "optional fields")
{
    $getparam.='&'.$keys.'='.$this->eazypay_aes128Encrypt($values,$aes_key);
    $getparam1.='&'.$keys.'='.$values;
}
else
{
    //endata += item.Key + "";
    $getparam.='&'.$keys.'='.$values;
    $getparam1.='&'.$keys.'='.$values;
}
     
}
        $this->roll_log_insert($refno,$submerchantid,$pgamount,$stname,$email,$mob,$regno,$rollno,$uid,$fcode,$coursecode,$semester,$mode,null,null,null,null,$papers,$faculty,$token,$provfees,$degfees,$confees,$coursename,$collegecode,$collegename,$fine);

     header('Location:'.$eazypay_Payment_url.$getparam);
//echo $eazypay_Payment_url.$getparam1 ."/n".$eazypay_Payment_url.$getparam;
    }
    
    
	public function redirect($url)
	{
		header("Location: $url");
	}



} ?>