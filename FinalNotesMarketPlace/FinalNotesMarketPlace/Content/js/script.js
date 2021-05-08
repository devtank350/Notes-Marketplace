
//Login Form
$(".toggle-password").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input = $($(this).attr("toggle"));
  if (input.attr("type") == "password") {
    input.attr("type", "text");
  } else {
    input.attr("type", "password");
  }
});






//Signup Form
$(".toggle-password1").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input1 = $($(this).attr("toggle"));
  if (input1.attr("type") == "password") {
    input1.attr("type", "text");
  } else {
    input1.attr("type", "password");
  }
});

$(".toggle-password2").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input2 = $($(this).attr("toggle"));
  if (input2.attr("type") == "password") {
    input2.attr("type", "text");
  } else {
    input2.attr("type", "password");
  }
});




















//ChangePassword Form
$(".toggle-password11").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input11 = $($(this).attr("toggle"));
  if (input11.attr("type") == "password") {
    input11.attr("type", "text");
  } else {
    input11.attr("type", "password");
  }
});


$(".toggle-password12").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input12 = $($(this).attr("toggle"));
  if (input12.attr("type") == "password") {
    input12.attr("type", "text");
  } else {
    input12.attr("type", "password");
  }
});


$(".toggle-password13").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input13 = $($(this).attr("toggle"));
  if (input13.attr("type") == "password") {
    input13.attr("type", "text");
  } else {
    input13.attr("type", "password");
  }
});





//Header//Navigation


//Navigation
$(function(){
    showHideNav();
    $(window).scroll(function(){
        showHideNav();
    });
    function showHideNav(){
        if($(window).scrollTop()>50){
            $('nav').addClass('white-nav-top');
            $('.navbar-brand img').attr('src','/Content/images/home/logo.png')
//            $('#mobile-nav-open-btn').attr('color','blue')
            
            //Show backto top button
            $('#back-to-top').fadeIn();
        }else{
           $('nav').removeClass('white-nav-top');
            $('.navbar-brand img').attr('src','/Content/images/home/top-logo.png')
            $('#back-to-top').fadeOut();
            
        }
    }
    
});
//Smooth Scrolling
$(function(){
    $('a.smooth-scroll').click(function(event){
        event.preventDefault();
        var section_id = $(this).attr('href');
        $('html,body').animate({
           scrollTop: $(section_id).offset().top-64
        },1250,'easeInOutExpo');
    });
});
//Mobilemenu
$(function(){
    //Show Menubar
    $('#mobile-nav-open-btn').click(function(){
       $('#mobile-nav').css('height','100%'); 
    });
    //Hide Menubar
    $('#mobile-nav-close-btn,#mobile-nav a').click(function(){
       $('#mobile-nav').css('height','0%'); 
    });
});


// Get the modal
var modal = document.getElementById("myModal");

// Get the button that opens the modal
var btn = document.getElementById("nd-btn");

// Get the <span> element that closes the modal
var span = document.getElementsByClassName("close")[0];

// When the user clicks on the button, open the modal
btn.onclick = function() {
  modal.style.display = "block";
}

// When the user clicks on <span> (x), close the modal
span.onclick = function() {
  modal.style.display = "none";
}

// When the user clicks anywhere outside of the modal, close it
window.onclick = function(event) {
  if (event.target == modal) {
    modal.style.display = "none";
  }
}


$('#myModal').on('shown.bs.modal', function () {
  $('#myInput').trigger('focus')
})


//Context 
var coll = document.getElementsByClassName("collapsible");
var i;

for (i = 0; i < coll.length; i++) {
  coll[i].addEventListener("click", function() {
    this.classList.toggle("active");
    var content = this.nextElementSibling;
    if (content.style.display === "block") {
      content.style.display = "none";
    } else {
      content.style.display = "block";
    }
  });
}













// Get the modal for add review
var modal1 = document.getElementById("myModal1");

// Get the button that opens the modal
var btn1 = document.getElementById("nd-btn1");

// Get the <span> element that closes the modal
var span1 = document.getElementsByClassName("close1")[0];

// When the user clicks on the button, open the modal
btn1.onclick = function() {
  modal1.style.display = "block";
}

// When the user clicks on <span> (x), close the modal
span1.onclick = function() {
  modal1.style.display = "none";
}

// When the user clicks anywhere outside of the modal, close it
window.onclick = function(event) {
  if (event.target == modal1) {
    modal1.style.display = "none";
  }
}








/* ========================================================================================================== */
//                      FAQ
/* ========================================================================================================== */
$(document).ready(function () {
    // Add minus icon for collapse element which is open by default
    $(".collapse.show").each(function () {
        $(this).prev(".card-header").find(".img-faq").attr("src", "../../Content/images/FAQ/add.png");
        $(this).parentsUntil(".card").css({
            "border": "1px solid #d1d1d1"
        });

    });

    // Toggle plus minus icon on show hide of collapse element
    $(".collapse").on('show.bs.collapse', function () {

        $(this).prev(".card-header").find(".img-faq").attr("src", "../Content/images/FAQ/minus.png").css({
            "height": "23px",
            "width": "35px"
        });
        $(this).prev(".card-header").find("h6").css({
            "font-weight": "600"
        });
        $(this).prev(".card-header").css({
            "background": "white"
        });
        $(this).parent(".card").css("border", "1px solid #d1d1d1");

    }).on('hide.bs.collapse', function () {
        $(this).prev(".card-header").find(".img-faq").attr("src", "../Content/images/FAQ/add.png").css({
            "height": "38px",
            "width": "38px"
        });
        $(this).prev(".card-header").find("h6").css({
            "font-weight": "400"
        });
        $(this).prev(".card-header").css({
            "background": "#f3f3f3"
        });
        $(this).parent(".card").css("border", "none");

    });
});





































































