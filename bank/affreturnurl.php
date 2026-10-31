<?php
session_start();
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);
$res =  $_REQUEST;
//print_r($res);
//$st=print_r($res,true);
$aes_key='1162121111101159';
$str=$res['mandatory_fields'];
$mid = $res['ID'];
$responsecode = $res['Response_Code'];
$tstatus="N";
$transactionamount= '';
$uniquerefnumber='';
$transactiondate='';
if(strtoupper(trim($responsecode))=="E000")
{
$uniquerefnumber = $res['Unique_Ref_Number'];
$servicetaxamount = $res['Service_Tax_Amount'];
$processingfee = $res['Processing_Fee_Amount'];
$totalamount = $res['Total_Amount'];
$transactionamount = $res['Transaction_Amount'];
$transactiondate = $res['Transaction_Date'];
$interchangevalue = $res['Interchange_Value'];
$tdr = $res['TDR'];
$paymode = $res['Payment_Mode'];
$submerchantid = $res['SubMerchantId'];
$refno = $res['ReferenceNo'];
$tps = $res['TPS'];
$hrs = $res['RS'];
$hashData = "$mid|$responsecode|$uniquerefnumber|$servicetaxamount|$processingfee|$totalamount|$transactionamount|";
$hashData  .= "$transactiondate|$interchangevalue|$tdr|$paymode|$submerchantid|$refno|$tps|$aes_key";
$Hstring  = hash("SHA512",$hashData);
if($Hstring ==$hrs)
{
    $tstatus="Y";
    $bankstatus= "Success";
}
$_SESSION["transactionamount"] = $transactionamount;
$_SESSION["uniquerefnumber"] =$uniquerefnumber;
$_SESSION["transactiondate"] =$transactiondate;
}else
{
    $bankstatus="Declined ";
}
if($str!="")
{
    $words = explode("|", $str);
    $refno = $words[0];
    $submerchantid =  $words[1];
    $pgamount =  $words[2];
    $stname = $words[3];
    $regno = $words[4];
    $uid = $words[5];
    $fcode = $words[6];
    $mob = $words[7];
    $email = $words[8];
    
}

require_once 'useraff.php';



if($uid == "AFFILIATED(EXAM FEE)")
{
$user = new USER();
$user->bank_status_update($refno,$transactionamount,$uniquerefnumber,$transactiondate,$bankstatus);                     
}

if($uid == "AFFILIATED(CONDONATION FEE)")
{
$user = new USER();
$user->bank_status_update($refno,$transactionamount,$uniquerefnumber,$transactiondate,$bankstatus);                     
}

$_SESSION["refno"] =$refno;
$_SESSION["stname"] = $stname ;
$_SESSION["regno"] =$regno;
$_SESSION["mob"] =$mob;
$_SESSION["email"] =$email;
$_SESSION["bankstatus"] =$bankstatus;


if($uid == "AFFILIATED(EXAM FEE)")
{
header("Location:"."paymentreport_aff.php");
}
if($uid == "AFFILIATED(CONDONATION FEE)")
{
header("Location:"."paymentreportcon_aff.php");
}

exit();
?>