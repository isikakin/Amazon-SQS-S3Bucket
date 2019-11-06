const AWS = require('aws-sdk');

const s3 = new AWS.S3({});

exports.handler = async (event, context, callback) => {
    try {
        console.log("event", event);
        var bucketKey = event.Records[0].body;
        console.log('bucketKey', bucketKey);

        var params = {
            Bucket: 'test-akin',
            Key: bucketKey + '.json'
        };

        console.log(params);

        await s3.getObject(params).promise()
            .then(data => {
                console.log('getobject from s3');
                console.log("data", data.Body.toString('utf-8'));
                returnResponse(true, "Success", data.Body.toString('utf-8'));
            }).catch(err => {
                console.log("errorr", err);
                returnResponse(false, "Error", JSON.stringify(err));
            });
    }
    catch (err) {
        console.log('err', JSON.stringify(err.message));
        returnResponse(false, "Error", JSON.stringify(err.message));
    }

    function returnResponse(status, message, data) {
        let response = {
            status: status,
            message: message,
            data: data
        };
        return response;
    }
};
