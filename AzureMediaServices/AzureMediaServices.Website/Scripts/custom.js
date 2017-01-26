
var AZ = AZ || {};

AZ.WebApiUrl = 'http://localhost:52961';

// This method is used to post video to web api.
AZ.PostVideo = function () {
    var formData = new FormData();
    var myFilePicker = $('#myFile')[0];
    formData.append("myVideo", myFilePicker.files[0]);
    formData.append("Title", "VideoTitle");
    formData.append("StartDate", (new Date()).toUTCString());
    formData.append("EndDate", (new Date()).toUTCString());
    formData.append("Keywords", "Test");
    formData.append("IsFeatured", true);

    $.ajax({
        url: AZ.WebApiUrl + '/Media/PostVideo',
        type: 'POST',
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        error: onError,
        success: onSuccess,
        xhrFields: {
            withCredentials: true
        },
        xhr: function () {
            var xhr = new window.XMLHttpRequest();
            xhr.withCredentials = true;
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total;
                    var perc = (Math.round(percentComplete * 100));
                    if (perc < 100) {
                        $('#myModal').modal();
                        $('.progress-bar').css('width', perc + '%').attr('aria-valuenow', perc);
                    }
                    else if (perc = 100) {
                        $('#myModal').modal();
                        $('.progress-bar').attr('class', 'progress-bar progress-bar-success progress-bar-striped');
                    }
                }
            }, false);
            return xhr;
        },
    });

    function onSuccess(data, status, jqXHR) {
        var modal = $('#myModal');
        var assetId = data;
        modal.find('#progressStatus').html('<p>Thanks for uploading the video. You will be redirected to the next screen to track the video upload progress</p>');
        setInterval(function () {
            window.location.href = '/encodeprogress.html?assetId=' + assetId;
        }, 1000);
    };
    function onError(jqXHR, status, errorThrown) {
        var modal = $('#myModal');
        modal.find('#progressStatus').html('<p>Error: ' + errorThrown + '</p>');
    };
}

AZ.GetEncodeProgress = function () {

    var assetId = location.search.split('assetId=')[1];

    $.ajax({
        url: AZ.WebApiUrl + '/Media/GetEncodingProgress/?assetId=' + assetId,
        type: 'GET',
        cache: false,
        contentType: false,
        error: onError,
        success: onSuccess,
        xhrFields: {
            withCredentials: true
        }
    });

    function onSuccess(data, status, jqXHR) {
              
        var assetId = location.search.split('assetId=')[1];
        $('#videoAssetId').val(assetId);
        $('#jobState').val(data.State);
        $('#customMessage').val('');
        if (data.State == 'Finished') {
            $('#customMessage').val('Encoding job is finished and you will be redirected to fetch the stream url.');
            setInterval(function () {
                window.location.href = '/streamurl.html?assetId=' + assetId;
            }, 10000);
        } else if (data.State == 'Error') {
            $('#customMessage').val('Error in encoding job. Error: ' + data.Message);
        }
        else
        {
            $('#customMessage').val('This screen will refresh every 10 seconds to fetch the encoding progress.');
            setInterval(function () {
                window.location.href = '/encodeprogress.html?assetId=' + assetId;
            }, 10000);
        }
    };
    function onError(jqXHR, status, errorThrown) {
        debugger;
        $('#customMessage').val('Error: ' + errorThrown);      
    };
}

AZ.GetStreamUrl = function () {
    debugger;
    var assetId = location.search.split('assetId=')[1];

    $.ajax({
        url: AZ.WebApiUrl + '/Media/GetStreamUrl/?assetId=' + assetId,
        type: 'GET',
        cache: false,
        contentType: false,
        error: onError,
        success: onSuccess,
        xhrFields: {
            withCredentials: true
        }
    });

    function onSuccess(data, status, jqXHR) {  
        if (data.State == 'Finished') {
            $('#streamUrl').val(data.StreamUrl);
            $('#customMessage').val('Video is read for streaming')
        } else if (data.State == 'Error') {
            $('#streamUrl').val('');
            $('#customMessage').val('Error in encoding job. Error: ' + data.Message);
        }
        else {
            $('#streamUrl').val('');
            $('#customMessage').val('This screen will refresh every 10 seconds to fetch the stream url.');
            setInterval(function () {
                window.location.href = '/streamurl.html?assetId=' + assetId;
            }, 10000);
        }
             
    };
    function onError(jqXHR, status, errorThrown) {       
        $('#customMessage').val('Error: ' + errorThrown);
    };
}


