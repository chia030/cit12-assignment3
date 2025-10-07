public class Response
// Chiara: fixed Response to match CJTP
{
    public string Status { get; set; } = "1 Ok";
    public string? Body { get; set; }
}

/*

=== STATUS VALID OPTIONS ===
1 | Ok
2 | Created
3 | Updated
4 | Bad Request
5 | Not Found
6 | Error

=== 4 Bad Request body format ===
4 missing <element>, illegal <element>, ...

*/
