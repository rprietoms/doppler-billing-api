# Billing API

By the moment, this API will allow us to access to our customer's invoices and
invoices PDF.

## Continuous Deployment to test and production environments

We are following the same criteria than [DopplerForms](https://github.com/MakingSense/doppler-forms/blob/master/README.md#continuous-deployment-to-test-and-production-environments).

## API draft

```http
###
GET /accounts/{doppler/relay}/{id}/invoices?from=2020-01-01T00:00:00Z&to=2020-07-01T00:00:00Z
Authorization: Bearer {{token-jwt}}
​
{
    "invoices": [
        {
            "product": "doppler",
            "accountId": 22235,
            "date": "2020-01-01T00:00:00Z",
            "currency": "ARS",
            "amount": 128,
            // There will probably be more data
            "link": "//path_on_server/filename.ext"
        },
        // . . .
    ],
    //TBD
    "_links": [
        {
            "rel": "next",
            "path?": "/accounts/doppler/22235/invoices?from=2020-02-01T00:00:00Z&to=2020-07-01T00:00:00Z"
        }
    ]
}
​
###
GET /accounts/doppler/22235/invoices/501_20200508_130218.pdf
Authorization: Bearer {{token-jwt}}
​​
// Return PDF content with `application/pdf` content type

```

By the moment we could require JWT tokens with `isSu=true` and do not validate
if user is allowed to access to the requested data.
