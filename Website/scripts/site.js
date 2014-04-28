$('.search-results').Tapirus('535e598cbcdd720200000000', { inputSelector: '#searchInput' });
$('.search-results').Tapirus('535e598cbcdd720200000000', { inputSelector: '#searchInput2' });
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