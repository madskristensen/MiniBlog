$('.search-results').Tapirus('54380c043493cda4d1000003', { inputSelector: '#searchInput' });
$('.search-results').Tapirus('54380c043493cda4d1000003', { inputSelector: '#searchInput2' });
$('#searchform2').hide();
$('#searchform').submit(function () {
	$('#searchform2').show();
	$('#searchform').hide();
	$('html, body').animate({
			scrollTop: $('#searchform2').offset().top - 20
	}, 'slow');
	
});
$('#searchform2').submit(function () {
	$('html, body').animate({
		scrollTop: $('#searchform2').offset().top - 20
	}, 'slow');
	$('#searchform2').val('');
});