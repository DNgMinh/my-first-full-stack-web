$(document).ready(function() {
    // gán các sự kiện cho các element:
    initEvents();

    // Load dữ liệu:
    loadData();
})

var employeeId = null;
var employeeCode = null;
var formMode = "add";
var keyword = "";
var limit = 10;
var page = "1";

function loadData() {
    // Gọi api thực hiện lấy dữ liệu:
    $.ajax({
        type: "GET",
        async: false,
        url: `http://localhost:17024/api/Employees/Filter?keyword=${keyword}&limit=${limit}&page=${page}`,
        success: function(res) {
            $("table#tbEmployeeList tbody").empty();
            // Xử lý dữ liệu từng đối tượng:
            let ths = $("table#tbEmployeeList thead th");
            for (const emp of res) {
                // duyệt từng cột trong tiêu đề:
                var trElement = $('<tr></tr>');
                for (const th of ths) {
                    // Lấy ra propValue tương ứng với các cột:
                    const propValue = $(th).attr("propValue");

                    const format = $(th).attr("format");
                    // Lấy giá trị tương ứng với tên của propValue trong đối tượng:
                    let value = null;
                    value = emp[propValue];
                    let classAlign = "";
                    switch (format) {
                        case "date":
                            value = formatDate(value);
                            classAlign = "text-align--center";
                            break;
                        case "gender":
                            if (value==0) {value= "Nam"}
                            if (value==1) {value= "Nữ"}
                            if (value==2) {value= "Khác"}
                            break;
                    //     case "money":
                    //         value = Math.round(Math.random(100) * 1000000);

                    //         value = formatMoney(value);
                    //         classAlign = "text-align--right";
                    //         break;
                        default:
                            break;
                    }

                    // Tạo thHTML:
                    let thHTML = `<td class='${classAlign}'>${value||""}</td>`;
                    if (propValue == "function") {
                        thHTML = `<td class='${classAlign}'> <div class="dropdown">
                        <button onclick="myFunction('${emp.employeeID}')" class="dropbtn dropdown"></button>
                        <div id='${emp.employeeID}' class="dropdown-content dropdown">
                          <a class = "dropdown">Nhân bản</a>
                          <a class="delete dropdown">Xóa</a>
                          <a class = "dropdown">Ngưng sử dụng</a>
                        </div>
                        </div>  
                        </td>`;
                    }
                    // Đẩy vào trHMTL:
                    trElement.append(thHTML);
                }
                $(trElement).data("id", emp.employeeID);
                $(trElement).data("entity", emp);

                $("table#tbEmployeeList tbody").append(trElement)                
            }
        },
        error: function(res) {
            console.log(res);
        }
    });
}


function formatDate(date) {
    try {
        if (date) {
            date = new Date(date);

            // Lấy ra ngày:
            dateValue = date.getDate();
            dateValue = dateValue < 10 ? `0${dateValue}` : dateValue;

            // lấy ra tháng:
            let month = date.getMonth() + 1;
            month = month < 10 ? `0${month}` : month;

            // lấy ra năm:
            let year = date.getFullYear();

            return `${dateValue}/${month}/${year}`;
        } else {
            return "";
        }
    } catch (error) {
        console.log(error);
    }
}

function yyyymmdd(date) {
    try {
        if (date) {
            date = new Date(date);

            // Lấy ra ngày:
            dateValue = date.getDate();
            dateValue = dateValue < 10 ? `0${dateValue}` : dateValue;

            // lấy ra tháng:
            let month = date.getMonth() + 1;
            month = month < 10 ? `0${month}` : month;

            // lấy ra năm:
            let year = date.getFullYear();

            return `${year}-${month}-${dateValue}`;
        } else {
            return "";
        }
    } catch (error) {
        console.log(error);
    }
}



function initEvents() {

    $('.paging__number').click( function() {
        // $(this).classList.add('paging__number--selected')
        $(this).addClass('paging__number--selected')
        // $(this).siblings().classList.remove('row-selected');
        $(this).siblings().removeClass('paging__number--selected')
        // let pages = $('.paging__number--selected')
        // page = pages.value
        page = this.value;
        loadData()
    })

    $('.page__limit').click( function() {
        limit = this.value;
        loadData();
    })

    
    // $('.paging__number').click( function() {
    //     $(this).classList.add('paging__number--selected')
    //     $(this).siblings().classList.remove('paging__number--selected')
    //     limit = $(this).value
    //     loadData
    // })

    const search = document.getElementById("search");
    search.addEventListener("keyup", function() {
        keyword = search.value;
        loadData();
    });

    // Xoa
    $("#btnDelete").click(function() {
        $('#confirmDelete').html(`Bạn có chắc chắn muốn xóa nhân viên ${employeeCode} không?`)
        $("#dialogDelete").show();
    });

    $(document).on('click', '.delete', function() {
        employeeId = $(this).parent().attr('id');
        employeeCode = $(this).parent().parent().parent().parent().data('entity').employeeCode;
        console.log(employeeCode);
        $('#confirmDelete').html(`Bạn có chắc chắn muốn xóa nhân viên ${employeeCode} không?`)
        $("#dialogDelete").show();
    });

    $("#btnOkDelete").click(function() {
        // Gọi api thực hiện xóa:
        $.ajax({
            type: "DELETE",
            url: "http://localhost:17024/api/Employees/" + employeeId,
            success: function(response) {                
                $("#dialogDelete").hide();
                alert("Xoa thanh cong");
                // Load lại dữ liệu:
                loadData();             
            },
            error: function(response) {
                console.log(response);
                alert("Xoa du lieu fail");
            }           
        });
    });

    $("#btnCancelDelete").click(function() {
        $("#dialogDelete").hide();
    });

    $("#btnCancelAdd").click(function() {
        $("#dlgEmployeeDetail").hide();
    });

    $("#btnSave").click(function(){saveData("save")});
    $("#btnSaveAndAdd").click( function(){saveData("save&add")});

    $(document).on('dblclick', 'table#tbEmployeeList tbody tr', function() {
        formMode = "edit";
        // Hiển thị form:
        $("#dlgEmployeeDetail").show();

        // Focus vào ô input đầu tiên:
        $("#dlgEmployeeDetail input")[0].focus();

        // Binding dữ liệu tương ứng với bản ghi vừa chọn:
        let data = $(this).data('entity');
        employeeId = $(this).data('id');
        employeeCode = $(this).data('entity').employeeCode;
        // Duyệt tất cả các input:
        let inputs = $("#dlgEmployeeDetail input, #dlgEmployeeDetail select, #dlgEmployeeDetail textarea");
        for (const input of inputs) {
            // Đọc thông tin propValue:
            const propValue = $(input).attr("propValue");
            const type = $(input).attr("type");            
            let value = data[propValue];
            if (type == "date") {value = yyyymmdd(value)}
            input.value = value;
        }
    });

    $(document).on('click', 'table#tbEmployeeList tbody tr', function(event) {
        // Xóa tất cả các trạng thái được chọn của các dòng dữ liệu khác:
        $(this).siblings().removeClass('row-selected');
        if ($(event.target).is('.dropdown')) { 
            employeeId = null;
            employeeCode = null;
            return;
        };
        // In đậm dòng được chọn nếu chưa đậm. Bỏ in đậm nếu đã đậm rồi:
        if (this.classList.contains("row-selected")) {
            this.classList.remove('row-selected');
            employeeId = null;
            employeeCode = null;
        }
        else {
            this.classList.add("row-selected");
            employeeId = $(this).data('id');
            employeeCode = $(this).data('entity').employeeCode;
        }   
    });
   
    // Gán sự kiện click cho button thêm mới nhân viên:
    $("#btnAdd").click(showdlgEmployeeDetail);


    $(".dialog__button--close").click(function() {
        // Ẩn dialog tương ứng với button close hiện tại:
        $(this).parents(".dialog").hide();
    });


    // Nhấn đúp chuột vào 1 dòng dữ liệu (tr) thì hiển thị form chi tiết thông tin nhân viên:

    // Nhấn button xóa thì hiển thị cảnh báo xóa.

    // Nhấn button Refresh thì load lại dữ liệu:

    // Thực hiện validate dữ liệu khi nhập liệu vào các ô input bắt buộc nhập:


    $(".require").blur(function() {
        // Lấy ra value:
        var value = this.value;
        // Kiểm tra value:
        if (!value) {
            // ĐẶt trạng thái tương ứng:
            // Nếu value rỗng hoặc null thì hiển thị trạng thái lỗi:
            $(this).addClass("input--error");
            $(this).attr('title', "Thông tin này không được phép để trống");
        } else {
            // Nếu có value thì bỏ cảnh báo lỗi:
            $(this).removeClass("input--error");
            $(this).removeAttr('title');
        }
    })

    $('input[type=email]').blur(function() {
        // Kiểm tra email:
        var email = this.value;
        var isEmail = checkEmailFormat(email);
        if (!isEmail) {
            console.log("Email KHÔNG đúng định dạng");
            $(this).addClass("input--error");
            $(this).attr('title', "Email không đúng định dạng.");
        } else {
            console.log("Email đúng định dạng");
            $(this).removeClass("input--error");
            $(this).removeAttr('title', "Email không đúng định dạng.");
        }
    })
}

function saveData(save) {
    // Thu thập dữ liệu:
    let inputs = $("#dlgEmployeeDetail input, #dlgEmployeeDetail select");
    var employee = {};

    // build object:
    for (const input of inputs) {
        // Đọc thông tin propValue:
        const propValue = $(input).attr("propValue");
        // Lấy ra value:
        if (propValue) {
            let value = input.value;
            if (value == "") {value = null};
            employee[propValue] = value;
        }
    }

    // Gọi api thực hiện cất dữ liệu:
    if (formMode == "edit") {
        $.ajax({
            type: "PUT",
            url: "http://localhost:17024/api/Employees/" + employeeId,
            data: JSON.stringify(employee),
            dataType: "json",
            contentType: "application/json",
            success: function(response) {
                alert("Sửa dữ liệu thành công!");
                // load lại dữ liệu:
                loadData();                
                if (save == "save") {
                    $("#dlgEmployeeDetail").hide();
                }
                if (save == "save&add") {
                    showdlgEmployeeDetail();
                }
            },
            error: function(res) {
                alert("Sửa dữ liệu thất bại!");
                console.log(res);
            }
        });
    } else {
        $.ajax({
            type: "POST",
            url: "http://localhost:17024/api/Employees/",
            data: JSON.stringify(employee),
            dataType: "json",
            contentType: "application/json",
            success: function(response) {
                alert("Thêm mới dữ liệu thành công!");
                // load lại dữ liệu:
                loadData();              
                if (save == "save") {
                    $("#dlgEmployeeDetail").hide();
                }
                if (save == "save&add") {
                    showdlgEmployeeDetail();
                }
            },
            error: function(response) {
                alert("Thêm dữ liệu thất bại!");
                console.log(response);
            }
        });
    }
}

function showdlgEmployeeDetail() {
    formMode = "add";
    // Hiển thị form nhập thông tin chi tin chi tiết:
    $("#dlgEmployeeDetail").show();
    $('#dlgEmployeeDetail input').val(null);
    $('#dlgEmployeeDetail select').val(null);
    $('#dlgEmployeeDetail textarea').val(null);
    // Lấy mã nhân viên mới:
    $.ajax({
        url: "http://localhost:17024/api/Employees/NewEmployeeCode",
        method: "GET",
        success: function(newEmployeeCode) {
            $("#txtEmployeeCode").val(newEmployeeCode);
            $("#txtEmployeeCode").focus();
        }
    });
}

function checkEmailFormat(email) {
    const re = /^(([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
    return email.match(re);
}


/* When the user clicks on the button,
toggle between hiding and showing the dropdown content */
function myFunction(id) {
    document.getElementById(id).classList.toggle("show");
  }
  
  // Close the dropdown menu if the user clicks outside of it
window.onclick = function(event) {
    if (!event.target.matches('.dropbtn')) {
        var dropdowns = document.getElementsByClassName("dropdown-content");
        var i;
        for (i = 0; i < dropdowns.length; i++) {
            var openDropdown = dropdowns[i];
            if (openDropdown.classList.contains('show')) {
                openDropdown.classList.remove('show');
            }
        }
    }
}



//   if (!event.target.matches('.dropbtn')) {
