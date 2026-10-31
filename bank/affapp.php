<?php

//ini_set('display_errors', 1);
//ini_set('display_startup_errors', 1);
//error_reporting( E_ALL );
if(isset($_POST['save']))
{
    require_once 'useraff.php';
  
 
    $user = new USER();
    $submerchantid = '55';
    $pgamount= $_POST["Mdemand"];
    $stname=$_POST["Mname"];
    $email='';
    $mob='';
    $regno=$_POST["Mregno"];
    $rollno='';
    $uid="AFFILIATED(EXAM FEE)";
    $papers=$_POST["Mpapers"];
    $fcode='7151';
    $semester=$_POST["Msem_no"];
    $coursecode=$_POST["Mcoursecode"];
    $coursename=$_POST["Mcoursename"];
    $collegecode=$_POST["Mcollegecode"];
    $collegename=$_POST["Mcollegename"];
    $mode= 'AFFILIATED(EXAM FEE)';
    $faculty='';
    $token='';
    $degchecked='';
    $conchecked='';
    $provfee='';
    $degfee = 0;
    $confee = 0;
    $fine = 330;
    
    
    if($pgamount == 0 or $stname == '')
{
    echo "<script>
    
alert('Please Select Paper or Name or Faculty');


</script>";
 
}
else
{
     
   
    if(false) 
    {
        $user->posttobankregular($submerchantid,$pgamount,$stname,$email,$mob,$regno,$rollno,$uid,$fcode,$semester,$coursecode,$mode,$papers,$faculty,$token,$provfee,$degfee,$confee,$coursename,$collegecode,$collegename,$fine);
   
   } 
  else
    {
        echo "<script>
//alert('try after some times');
    alert('Application Registration Closed');
    </script>";
        
    }

    
}
}
?>
<noscript>
    This page needs JavaScript activated to work.
    <style>
        div {
            display: none;
        }
    </style>
</noscript>


 

  <head><meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
      <link href="/css/Site.css" rel="stylesheet" />
    
     <link href="/css/bootstrap.min.css" rel="stylesheet">

      <link href="/css/Website.css" rel="stylesheet" />
      
      <link href="/css/Site.css" rel="stylesheet" />
     <script src="/Scripts/kendo/2018.1.117/jquery.min.js"></script>
       <script src="/js/bootstrap.min.js"></script>
    
       <title>Affliated Exam Fee</title>
  </head>
 
 
 
 <body>
 

<style type="text/css">
 
 
    th, td {
        padding-top: 10px;
        padding-bottom: 10px;
        padding-left: 10px;
        padding-right: 10px;
    }
     

img.animated-gif{
  width: 120px;
  height: auto;
}
 #loading{
  position: fixed;
  left: 0px;
  top: 0px;
  width: 100%;
  height: 100%;
  z-index: 9999;
  background: url('50px-Phi_fenomeni.gif') 
              50% 50% no-repeat rgb(199,215,242);opacity:.5;
}
</style>

<div id="loading"></div>

   <div class="container body-content">
        
        <div class="container">
 <img src="images/top2.jpg" alt="Smiley face" height="175" width="1300">
            <div class="panel panel-success">
                <div class="panel-heading">



                    <h4 align="center">
                        <i> Affiliated Colleges Examination Fee Online Payment Portal April/May - 2026</i>
                    </h4>
                    <!-- <h4 align="center">
                        Last Chance for those who are yet to pay for Final Semester only
                    </h4> -->

                </div><!--panel-heading -->
                <div class="panel-body">
                    <div id="example">
                       <!--   <h4 style="color:red;">Rollno Only for First Semester Students</h4>
                         <h4 style="color:blue;">Add Collegecode before rollno if it starts with 24</h4> -->

                <table>
                    <tr>
                        <td style="padding:5px;">
                            <label>Regno:</label>
                
                        </td>
                
                        <td>
                
                            <input type="text" id="mregno"  name="regno" />
                            <button id="search" type="button">Search</button>
                            
                        </td>
                        <td>
                            <div id="Error" style="color:red;font-size: 20px;"></div>
                        </td>
                        <td>
                            <div id="test" style="color:red;font-size: 20px;"> </div>
                            
                        </td>
                    </tr>
                   
                
                   
                
                </table>
        <table id="stddetail">
            <tr>
                <td>Collegecode:</td>
                <td id="collegecode"></td>
                <td>College Name:</td>
                <td id="collegename"></td>
            </tr>
            <tr>
                <td>Programmecode:</td>
                <td id="progcode"></td>
                <td>Programme Name:</td>
                <td id="progname"></td>
            </tr>
        
            <tr>
                <td>Name:</td>
                <td colspan="2" id="name"></td>
                
            </tr>
            <tr>
                <td>Register No/Rollno:</td>
                <td id="regno"></td>
            </tr>
            <tr>
                <td>Semester:</td>
                <td id="sem_no"></td>
            </tr>
            <tr>
                <td  style="color:red;">Exam Fee ₹:</td>
                <td style="color:blue;font-size: 20px;" id="demand"></td>
            </tr>
            <tr>
                <td ><img width="100" height="100" id="imgElem" /></td>
            </tr>
        
        </table>

            <div id="TableCont"></div>
            
                            <form id="step1" method="post">
                                  <input name="Mcollegecode"  type="hidden" id="Mcollegecode" />
                                  <input name="Mcollegename"  type="hidden" id="Mcollegename" />
                                  <input name="Mcoursecode"  type="hidden" id="Mcoursecode" />
                                  <input name="Mcoursename"  type="hidden" id="Mcoursename" />
                                  <input name="Mname"  type="hidden" id="Mname" />
                                  <input name="Mregno"  type="hidden" id="Mregno" />
                                  <input name="Msem_no"  type="hidden" id="Msem_no" />
                                  <input name="Mpapers"  type="hidden" id="Mpapers" />
                                  <input name="Mdemand"  type="hidden" id="Mdemand" />

     
                                      <div class="row">
                                        <div class="col-md-4"></div>
                                        <div class="col-md-11">
                                            <br />
                                           <!--     <h4 style="color:blue;">Registration (90) + Marksheet (90) + Fine(360) = ₹ 540/-</h4> -->
                                              <h4 style="color:blue;">Registration (85) + Marksheet (90)  = ₹ 175/-</h4>
                                              <h4 style="color:red;">Final Semester: </h4>
                                              <h4 style="color:blue;">Consolidate Mark Sheet (UG)-850/-, (PG)-1000/-</h4>
                                              <h4 style="color:blue;">Provisional Fees - 550/- </h4>
                                              <h4 style="color:blue;">Degree Fees - 700/-</h4>
                                            
    
                                            <button id="save" name="save" value="Save" type="submit" class="btn btn-warning btn-lg">Proceed Now</button>
    
    
    
                                        </div>
                                        <div class="col-md-4"></div>
                                        
                                    </div>
                            </form>

                            <div class="row">
                                
                                <div style="text-align:center;color:pink;">
                              
                            <!--  <a href="FEES.pdf" style="font-size: 20px; text-decoration: none;color:green" target="new">CLICK HERE FOR EXAM FEE DETAILS </a> -->
                              
                                    <label> 
                                <span style="font-size:large;color:green">Last Date for Payment: 30/04/2026 without fine</span></br>    
                                <span style="font-size:large;color:red">Last Date for Payment : 03/05/2026 with fine (Rs. 330)</span></br>    
                                <span style="font-size:large;color:blue">(In case of failure in online payment transaction, the students are advised to check acknowledgement after One day)</span>
                                </label>



                                </div>
                                    <hr />
                                    <div class="col-md-4">
                                        
                                        <div style="width:50px;margin-left:100px;">
                                            <img src="images/CW.png" />
                                        </div>
                                       
                                        <div style="font-weight:bold">
                                            <p style="color:red">
                                                <strong>Controller Office Grievance Cell</strong>
                                            </p>
                                            <p>Annamalai University</p>
                                            <p>Annamalai Nagar -608002</p>
                                            <p>Enq : 04144-238027</p>
                                            <p>Enq : 04144- 232282 Ext : 578</p>
                                            <p>
                                                Email:
                                                <a href="mailto:aucoecell2016@gmail.com">aucoecell2016@gmail.com</a>
                                            </p>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div style="width:125px;margin-left:100px;">
                                            <img src="images/icicibank.png" />
                                        </div>
                                        <div style="font-weight:bold">
                                            <p>NIL Charges for All Net Banking/Debit Card & Credit Card</p>
                                            <p>We accept Visa, Master, Mastero and Rupay Card</p>
                                            <p>Powered by ICICI Bank Gateway</p>

                                        </div>

                                    </div>
                                    <div class="col-md-4"></div>
                                </div>
                    </div><!--example -->

                </div><!--panel-body -->
            
            
            
                </div><!--panel-panel-sucess -->


        </div> <!--container -->
    </div><!--container body-content -->



</body>

<script type="text/javascript">

    $(document).ready(function () {

$("#save").hide();
$("#stddetail").hide();
  var $loading = $('#loading').hide();
  
                   //Attach the event handler to any element
                   $(document)
                     .ajaxStart(function () {
                        //ajax request went so show the loading image
                         $loading.show();
                     })
                   .ajaxStop(function () {
                       //got response so hide the loading image
                        $loading.hide();
                    });

      



    });
    $("#search").on("click", function () {
         if ($.trim($("#mregno").val()) == "") {
              $('#Error').html("Please Enter Register Number!");
                
                return;
            }
            
        SearchRegno();
    });
    $.makeTable = function (mydata) {
       
        var table = $('<table border=1>');
        var tblHeader = "<tr><th>Smester</th><th>Course Code</th><th>Course Title</th><th>Course Fees</th></tr>";
        //for (var k in mydata[0]) tblHeader += "<th>" + k + "</th>";
        //tblHeader += "</tr>";

        $(tblHeader).appendTo(table);
        $.each(mydata, function (index, value) {
            var TableRow = "<tr>";
            $.each(value, function (key, val) {
                TableRow += "<td>" +val + "</td>";
            });
            TableRow += "</tr>";
            $(table).append(TableRow);
        });
        return ($(table));
    };
    function makeRequest(){
 $.ajax(
                          {
                type: 'GET',
                url: "https://aucoeexam.in/api/getenroll",
                dataType: 'json',
                data: { regno: $.trim($("#mregno").val()) },


                success: function (result) {

                    // $('#jsondata').html(JSON.stringify(result));

                    
                    var mydata = eval(result);
                    if (mydata.length === 0) {
                        $("#save").hide();
                        $("#TableCont").empty();
                        $("#stddetail").hide();
                        $('#Error').html('Register Number Not Found!');
                        return;
                    }
                    $("#save").show();
                    $('#Error').html("");
                    $("#stddetail").show();
                    
                    $('#progcode').html(mydata[0].coursecode);
                    $('#progname').html(mydata[0].coursename);
                    $('#collegecode').html(mydata[0].collegecode);
                    $('#collegename').html(mydata[0].collegename);
                    $('#name').html(mydata[0].name);
                    $('#regno').html(mydata[0].regno);
                    if(mydata[0].sem_no == "9")
                    {
                        $('#sem_no').html("Private");
                    }
                    else
                    {
                         $('#sem_no').html(mydata[0].sem_no);
                    }
                   
                    $('#demand').html((parseInt(mydata[0].demand))+"/-");

                    imgElem.setAttribute('src', "data:image/jpg;base64," + mydata[0].photo1);
                    var table = $.makeTable(mydata[0].papers);
                    var allpapers= "";
                    $.each(mydata[0].papers, function (index, value) {
                                
                                $.each(value, function (key, val) {
                                    if(key === "pap_no")
                                    {
                                         allpapers += val+"|" ;
                                    }
                            });
                    });
                     
                     $("#Mcollegecode").val(mydata[0].collegecode);
                     $("#Mcollegename").val(mydata[0].collegename);
                     $("#Mcoursecode").val(mydata[0].coursecode);
                     $("#Mcoursename").val(mydata[0].coursename);
                     $("#Mname").val(mydata[0].name);
                     $("#Mregno").val(mydata[0].regno);
                     $("#Mpapers").val(allpapers);
                     $("#Mdemand").val(parseInt(mydata[0].demand));
                     $("#Msem_no").val(mydata[0].sem_no);
                     
                    
                     $('save').toggle(true);  
                    
                    $("#TableCont").empty();
                    $(table).appendTo("#TableCont");

                },
                error: function (request, status, error) {
                    $("#save").hide();
                      $('#Error').html("Something went wrong!");
                    

                }
            })
};
    function SearchRegno() {

hasrow = false;
$.ajax({
                type: 'POST',
                url: "affappVM.php",
                dataType: 'json',
                data:{linkdata:$.trim($("#mregno").val())},
                success: function (result) {
                    
                       var mydata = eval(result);
                       if(mydata.length === 0)
                       {
                           hasrow = true;
                        
                           
                       }
                       else
                       {
                        $("#save").hide();
                        $("#TableCont").empty();
                        $("#stddetail").hide();
                     
                           
                       }
                       
                $("div#test").empty();
                $('#Error').html("");
                       
                   $("div#test").append('<ul>');
                var ul = $("div#test > ul");

                 $.each(mydata, function (index, value) {
                    ul.append('<h3 ><a href="'+value.refno+'" style="color:blue" >Your Payment was successfull click here for acknowledgement</a>  </h3>');     
                 });


                  },
                   error: function (request, status, error) {
                    $("#save").hide();
                      $('#Error').html("Something went wrong!");
                    

                },
                complete: function (data) {
                        
                        if(hasrow)
                        {
                            makeRequest(); 
                        }
                }
    
                
            });

       


    }

</script>