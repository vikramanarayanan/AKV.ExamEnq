$(document).ajaxStart(function () {
    $('.loading-wrapper').show();
  
});

 $(document).ajaxStop(function () {
  
   $('.loading-wrapper').hide();
    
});


$(document).ready(function () {

     
   
    
    //$("#grade").kendoDropDownList({

    //    dataTextField: "name",
    //    dataValueField: "id",
    //    dataSource: [
    //        { id: "A", name: "A" },
    //        { id: "B", name: "B" },
    //        { id: "C", name: "C" },
    //        { id: "D", name: "D" },
    //        { id: "S", name: "S" },
    //        { id: "O", name: "O" }



    //    ],
    //    optionLabel: "Select Grade",

    //});

    
    
    $("#yearofpassing").kendoDatePicker({
        start: "decade",
        depth: "decade",
        format: "yyyy",
        
    });
    
    
    $("#gender").kendoDropDownList({
        valuePrimitive: true,
        dataTextField: "name",
        dataValueField: "id",
        dataSource: [{ id: "Male", name: "Male" }, { id: "Female", name: "Female" }, { id: "Transgender", name: "Transgender" }],
        optionLabel: "Select Gender",
         
    });
    $("#feepaid").kendoDropDownList({
        valuePrimitive: true,
        dataTextField: "name",
        dataValueField: "id",
        dataSource: [{ id: "Yes", name: "Yes" }, { id: "No", name: "No" }],
        optionLabel: "Select Yes/No",

    });
    $("#medium").kendoDropDownList({
        valuePrimitive: true,
        dataTextField: "name",
        dataValueField: "id",
        dataSource: [{ id: "English", name: "English" }, { id: "Tamil", name: "Tamil" }],
        optionLabel: "Select Medium",


    });
    
    $("#mobno").kendoMaskedTextBox({
        mask: "0000000000"
    });
    $("#ppincode").kendoMaskedTextBox({
        mask: "000000"
    });
    $("#cpincode").kendoMaskedTextBox({
        mask: "000000"
    });
    $("#aadharno").kendoMaskedTextBox({
        mask: "0000/0000/0000"
    });
    $("#totalmarks").kendoNumericTextBox({
        spinners: false,
        format: "#",
        decimals: 0
    });
    $("#Previous2").kendoButton();
    $("#Previous3").kendoButton();
    $("#Previous4").kendoButton();
    $("#Previous5").kendoButton();
    $("#Step1").kendoButton();
    $("#Step2").kendoButton();
    $("#Step3").kendoButton();
    $("#Step4").kendoButton();
    $("#Step5").kendoButton();
    var fileNotSelected = false;
    var progress,
    tabs,
    currentIndex = 0;

    function refreshImage(imgElement, imgURL) {
        // create a new timestamp 
        var timestamp = new Date().getTime();

        var el = document.getElementById(imgElement);

        var queryString = "?t=" + timestamp;

        el.src = imgURL + queryString;
    }
    function onShow1(e) {
        var mstep = $(e.item).find("> .k-link").text();
       
        if (mstep.trim() === "Step5")
        {
            var url1 = $("#imgphoto").attr("src");
            var url2 = $("#imgsignature").attr("src");
            var url3 = $("#imgpwd").attr("src");
            var url4 = $("#imgcommunity").attr("src");
            refreshImage("imgphoto", url1);
            refreshImage("imgsignature", url2);
            refreshImage("imgpwd", url3);
            refreshImage("imgcommunity", url4); 
            
        }
    }
    $("#tabstrip").kendoTabStrip({
        show: onShow1,
        animation: {
            open: {
                effects: "fadeIn"
            }
        }
    });
    progress = $("#profileCompleteness").data("kendoProgressBar");


    tabs = $("#tabstrip").data("kendoTabStrip");

    tabs.enable(getTabAtIndex(1), false);
    tabs.enable(getTabAtIndex(2), false);
    tabs.enable(getTabAtIndex(3), false);
    
    function getTabAtIndex(index) {
        return tabs.tabGroup.children().eq(index);
    }

    function tabIndexOfElement(element) {
        return tabs.element.find(element).index();
    }

    function isTabValidAt(tabIndex, e) {

        var el = tabs.contentElement(tabIndex),

            val = $(el).kendoValidator({
                rules: {
                    
                    
                      
                    upload: function (input) {
                        if (input[0].type === "file" && input[0].id == "uploadphoto") {
                            fileNotSelected = true;
                            var len = input.closest(".k-upload").find(".k-file").length;
                            var invalid = input.closest(".k-upload").find(".k-file-invalid").length;

                          //  return (len > 0 && invalid === 0);
                            return true;
                        }
                        
                        return true;
                    },
                    
                    validmask: function (input) {

                        if (input.is("[data-validmask-msg]") && input.val() !== "") {
                            var maskedtextbox = input.data("kendoMaskedTextBox");
                            return maskedtextbox.value().indexOf(maskedtextbox.options.promptChar) === -1;
                        }

                        return true;
                    },

                         
                    hasItems: function (input) {
                        if (input.is("[data-greaterdate-msg]") && input[0].id == "papselect") {
                            //Get the MultiSelect instance
                            var ms = input.data("kendoMultiSelect");
                            if (ms.value().length < 1) {
                                return true;
                            }
                        }
                        return true;
                    }

                },
                messages: {
                    upload: "Upload Image",
                    
                     hasItems: "Please select paper"
                    
                }

            }).data("kendoValidator");
        if (!val.validate()) {

            e.preventDefault()
        }
        return val.validate();


    }

    //READ APPLICATION FROM SERVER


    var viewModel = kendo.observable({
        photoFiles: [],
        papers: [],
      
        signatureFiles: [],
        pwdFiles: [],
        communityFiles: [],
        pwdvisible: true,
        pwddisable: true,
        communityvisible: true,
        communitydisable: true,
        coursecode:null,
        name: null,
        rollno:null,
        dateofbirth:null,
        gender: null,
        feepaid: null,
        pwd:null,
        category:null,
        centre: null,
        subject: null,
        scribe: null,
        yearofpassing: null,
        examfee: 0,
        mclass: null,
        test1: function () {
            return kendo.stringify(this.get("papers"));
        },
        source: new kendo.data.DataSource({

            //serverFiltering: true,
            transport: {

                read: {
                    url: '/RollGrid/GetSubmst/',
                    dataType: "json",
                }
            }
        }),

        path: function () {
            return "ddeappVM.php?papno=" + this.get("coursecode") + "&subyear=" + this.get("subyear");
        },

        Previous: function (e) {
            e.preventDefault();
            tabs.select(tabs.select().prev());
        },
        
        Step1: function (e) {
            e.preventDefault();
            tabs.select(getTabAtIndex(currentIndex + 1));
            if (viewModel.isvalidated) {

                 
                viewModel.set("gender", $("#gender").data("kendoDropDownList").text());
                 
                viewModel.set("category", $("#category").data("kendoDropDownList").text());
                
                $.ajax({
                    type: 'POST',
                    url: '/RollGrid/AppSave1/',
                    dataType: 'JSON',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        
                        gender: $("#gender").data("kendoDropDownList").text(),
                        medium: $("#medium").data("kendoDropDownList").text(),
                        category: $("#category").data("kendoDropDownList").text(),
                        mobileno: $("#mobno").data("kendoMaskedTextBox").value(),


                    }),


                    error: function (request, status, error) {
                        tabs.select(getTabAtIndex(currentIndex - 1));
                        alert("Something went wrong");
                       
                    }
                });

            }
        },

        Step2: function (e) {
            e.preventDefault();
            tabs.select(getTabAtIndex(currentIndex + 1));
            if (viewModel.isvalidated) {

                //viewModel.set("feepaid", $("#feepaid").data("kendoDropDownList").text());
                $.ajax({
                    type: 'POST',
                    url: '/RollGrid/AppSave2/',
                    dataType: 'JSON',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        papers: viewModel.papers
                       // papers: viewModel.papers, feepaid: viewModel.feepaid, examfee: viewModel.examfee
                       
                    }),

                    error: function (request, status, error) {
                        tabs.select(getTabAtIndex(currentIndex - 1));
                        alert("Something went wrong");

                    }
                });
            }

        }
        ,
         
        Step3: function (e) {
            e.preventDefault();
            tabs.select(getTabAtIndex(currentIndex + 1));
            if (viewModel.isvalidated) {
                javascript: window.open('', '_self').close();
              //  $(e).closest("[data-role=window]").data("kendoWindow").close();
               
            }
           

        }
        ,

        Step4: function (e) {
            e.preventDefault();

            tabs.select(getTabAtIndex(currentIndex + 1));

        },
        Step5: function (e) {
            e.preventDefault();
            tabs.select(getTabAtIndex(currentIndex + 1));
            if (viewModel.isvalidated) {
            kendo.confirm("You Cannot edit the Application Once Submitted !")
       .done(function () { 

               $.ajax({
                   url: '/Application/AppSave5/',
                   type: "POST",
                   dataType: 'json',
                   contentType: "application/json; charset=utf-8",
                   data: JSON.stringify({
                       ss: 'ss',

                   }),

                   error: function (request, status, error) {
                       tabs.select(getTabAtIndex(currentIndex - 1));
                       alert("Something went wrong");

                   },
                   success: function (data) {
                       window.location.href = data;
                   }
               });
               
       })
       .fail(function () {
           tabs.select(getTabAtIndex(currentIndex - 1));
       });
         }

           
        },
         
        onSelect: function (e) {

            var selectedIndex = tabIndexOfElement(e.item),

            isMovingBack = selectedIndex < currentIndex

            if (isMovingBack || isTabValidAt(currentIndex, e)) {
                // if (isTabValidAt(currentIndex, e)) {
                console.log("tab passed validation")
                viewModel.set("isvalidated", true);
                tabs.enable(getTabAtIndex(selectedIndex), true);
                tabs.enable(getTabAtIndex(currentIndex), false);
                currentIndex = selectedIndex;
            }
            else {
                viewModel.set("isvalidated", false);
                e.preventDefault();
            }
        },
        onchangeAdd: function(){

           
            viewModel.set("caddr1", viewModel.paddr1);
            viewModel.set("caddr2", viewModel.paddr2);
            viewModel.set("caddr3", viewModel.paddr3);
            viewModel.set("caddr4", viewModel.paddr4);
            viewModel.set("caddr5", viewModel.paddr5);
            viewModel.set("cpincode", viewModel.ppincode);
        },

        onShow: function (e) {
            
            viewModel.set("progressValue", currentIndex);
            // console.log("currentindex", currentIndex);
        },

        progressValue: 0,
        isvalidated:false,
        isEnabled: true,
        isVisible: true,
        onStart: function () {
            console.log("event :: start (" + this.get("progressValue") + ")");
        },
        onChange: function () {
            console.log("event :: change (" + this.get("progressValue") + ")");
        },
        onComplete: function () {
            console.log("event :: complete (" + this.get("progressValue") + ")");
        }

    });
    $("#papselect").kendoMultiSelect({
        autoClose: false
    });
    $("#category").kendoDropDownList({
        valuePrimitive: true,
        dataTextField: "categoryname",
        dataValueField: "categoryname",
        dataSource: {
            transport: {
                read: {
                    url: "/RollGrid/GetCategoryes",
                    dataType: "json"
                }
            }
        },
        optionLabel: "Select Category",


    });


    $("#course").kendoDropDownList({
        valuePrimitive: true,
        dataTextField: "coursename",
        dataValueField: "coursecode",
        dataSource: {
            transport: {
                read: {
                    url: "/RollGrid/GetCourses",
                    dataType: "json"
                }
            }
        },
        optionLabel: "Select Course",


    });
     
 

    $("#uploadphoto").kendoUpload({
        multiple: false,
        async: {
            saveUrl: "/RollGrid/Photosave",
            removeUrl: "/RollGrid/Photoremove",
            autoUpload: true
        },

          files: viewModel.photoFiles,

        upload: function (e) {
            var files = e.files;
            $.each(files, function () {

                if (this.extension.toLowerCase() != ".jpg") {
                    alert("Only .jpg files can be uploaded!")
                    e.preventDefault();
                }

                if (this.size / 1024 / 1024 > 100000.008) {
                    alert("Max 8kb file size is allowed!")
                    e.preventDefault();
                }
            });
            e.data = { guid: $('#guid').val() };

        },
        remove: function (e) {
            e.data = { guid: $('#guid').val() };
        },
        success: function (e) {
            // alert(viewModel.initialFiles);
            //                    alert(e.files);

            //              var files = e.files;
            var smallImage = $("#uploadphotoImage");

            if (e.operation == "upload") {
                //  alert("Status: " + e.response.status);

                smallImage.attr("src", "data:image/jpeg;base64," + e.response.status);
            } else {

                smallImage.attr("src", "data:image/jpeg;base64," + null);
            }
        }

    });
    $("#degclass").kendoDropDownList({
        valuePrimitive: true,
        dataTextField: "name",
        dataValueField: "id",
        dataSource: [{ id: "First", name: "First" }, { id: "Second", name: "Second" }],
        optionLabel: "Select Class",


    });
   

    kendo.bind($("#example"), viewModel);

    $.ajax(
      {
          type: 'GET',
          url: "/RollGrid/ReadByApplno",
          dataType: 'json',

          success: function (result) {


              //step1
         
             
              
               
              if (result.sex != null) {
                 
                  $("#gender").data("kendoDropDownList").text(result.sex);
              } else {
                  $("#gender").data("kendoDropDownList").value(null);
              }
              if (result.medium != null) {
                  $("#medium").data("kendoDropDownList").text(result.medium);
              } else {
                  $("#medium").data("kendoDropDownList").value(null);
              }
              if (result.community != null) {
                  $("#category").data("kendoDropDownList").text(result.community);
              } else {
                  $("#category").data("kendoDropDownList").value(null);
              }
               
              viewModel.set("coursecode", result.coursecode);
              viewModel.set("rollno", result.rollno);
              viewModel.set("name", result.name);
              viewModel.set("mobno", result.mobileno);
              viewModel.set("examfee", result.examfee);
              //viewModel.set("feepaid", result.paidornot);
              //$("#feepaid").data("kendoDropDownList").text(result.paidornot);
              viewModel.set("papers", []);
              // data1 = '[{ "coursecode": "101", "pap_no": "P102", "papname1": "PAPerr2", "SEMNO": "1" }]';
            //  alert($.parseJSON(result.papers));
            //  viewModel.papers.push.apply(viewModel.papers, $.parseJSON(result.papers));
 
              viewModel.papers.push.apply(viewModel.papers, result.papers);
              ///////

              if (result.image != "") {
                  viewModel.set("photoFiles", [{ "name": "photo", "extension": "", "size": 0 }]);
              }
              
               
              if (result.image != "") {
              var photoimage = $("#uploadphotoImage");
                  photoimage.attr("src", "data:image/jpeg;base64," + result.image);
                
            } 

          }, //ajax end

          error: function (request, status, error) {
              alert("Something went wrong");

          }





      }).done(function () {


          $("#uploadphoto").kendoUpload({
              multiple: false,
              async: {
                  saveUrl: "/RollGrid/Photosave",
                  removeUrl: "/RollGrid/Photoremove",
                  autoUpload: true
              },

              files: viewModel.photoFiles,

              upload: function (e) {
                  var files = e.files;
                  $.each(files, function () {

                      if (this.extension.toLowerCase() != ".jpg") {
                          alert("Only .jpg files can be uploaded!")
                          e.preventDefault();
                      }

                      if (this.size / 1024 / 1024 > 0.1) {
                          alert("Max 1MB file size is allowed!")
                          e.preventDefault();
                      }
                  });
                //  e.data = { guid: $('#guid').val() };

              },
              remove: function (e) {
                 // e.data = { guid: $('#guid').val() };
              },
              success: function (e) {
                  // alert(viewModel.initialFiles);
                  //                    alert(e.files);

                  //              var files = e.files;
                  var smallImage = $("#uploadphotoImage");

                  if (e.operation == "upload") {
                      //  alert("Status: " + e.response.status);

                      smallImage.attr("src", "data:image/jpeg;base64," + e.response.status);
                  } else {

                      smallImage.attr("src", "data:image/jpeg;base64," + null);
                  }
              }

          });



          
      });//ajax end

     

   



});


