$('.search-results').Tapirus('535e3924c4fe2b0200000005', { inputSelector: '#searchInput' });
$('.search-results').Tapirus('535e3924c4fe2b0200000005', { inputSelector: '#searchInput2' });
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