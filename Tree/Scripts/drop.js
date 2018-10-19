$(function () {
	$('.category-icon').click(function () {
		var isCollabsed = $(this).attr('data-collabsed') == 1;
		if (isCollabsed) {
			$(this).attr('data-collabsed', 0);
			$(this).parent().parent().find('ul:first').show();
			$(this).removeClass('glyphicon-plus');
			$(this).addClass('glyphicon-minus');
		}
		else {
			$(this).attr('data-collabsed', 1);
			$(this).parent().parent().find('ul:first').hide();
			$(this).removeClass('glyphicon-minus');
			$(this).addClass('glyphicon-plus');
		}

	});
});