<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <html>
      <head>
        <META http-equiv="Content-Type" content="text/html; charset=utf-8"/>
        <title>NLedger Test Execution Report</title>
        <style>
          div.background { background-color:ghostwhite; width:80%; margin:auto; padding:1px 20px 20px 20px; }
          div.container { background-color:lightgray; border: 2px solid darkgrey; }
          div.line { border-bottom: 1px solid darkgrey; display:inline-block; width:100%; }
          div.lastline { display:inline-block; width:100%; }
          div.item1 { border-right: 1px solid darkgrey; width:170px; float:left; }
          div.item200 { border-right: 1px solid darkgrey; width:200px; float:left; }
          div.item2 { border-right: 1px solid darkgrey; width:100px; float:left; }
          div.item3 { border-left: 1px solid darkgrey; width:240px; float:right; }
          div.item4 { border-left: 1px solid darkgrey; width:80px; float:right; }
          div.status { border-left: 1px solid darkgrey; width:100px; float:right; text-align:center; }
          div.separator { height:3px; }
          div.leftpanel { width:50%; float:left; }
          div.rightpanel { border-left: 1px solid darkgrey; overflow:hidden; }
          span.alert { color:red; }
          span.warn { color:darkorange; }
          pre.content { word-wrap:break-word; margin-top:0px; margin-bottom:0px; marging-left:2px; margin-right:2px; }
        </style>
      </head>
      <body>
        <div class="background">
          <h1>NLedger Test Execution Report</h1>
          <h2>Summary</h2>
          <xsl:apply-templates select="/nltest-results/summary" />
          <h2>Test Results</h2>
          <xsl:apply-templates select="/nltest-results/test-cases" />
        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="summary">

    <xsl:variable name="failed-style">
      <xsl:choose>
        <xsl:when test="@has-failed='True'">alert</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="ignored-style">
      <xsl:choose>
        <xsl:when test="@has-ignored='True'">warn</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <div class="container">
      <div class="line">
        <div class="item1">Report Date:</div>
        <div class="item200"><xsl:value-of select="@date-str"/></div>
        Date and time when the report was built
      </div>
      <div class="line">
        <div class="item1">Number of Tests:</div>
        <div class="item200"><xsl:value-of select="@total-tests"/></div>
        The number of test files selected according to the selection criteria
      </div>
      <div class="line">
        <div class="item1">Number of Test Cases:</div>
        <div class="item200"><xsl:value-of select="@total-test-cases"/></div>
        The total number of test cases found in all selected test files
      </div>
      <div class="line">
        <div class="item1">Total Execution Time:</div>
        <div class="item200"><xsl:value-of select="@total-time"/></div>
        The total time that NLedger was working. Testing Framework time is not included
      </div>
      <div class="line">
        <div class="item1">Passed test cases:</div>
        <div class="item200"><xsl:value-of select="@passed-tests"/> (<xsl:value-of select="@passed-tests-percent"/>%)</div>
        The total number of test cases that passed successfully
      </div>
      <div class="line">
        <div class="item1">Failed test cases:</div>
        <div class="item200"><span class="{$failed-style}"><xsl:value-of select="@failed-tests"/> (<xsl:value-of select="@failed-tests-percent"/>%)</span>
        </div>
        The total number of test cases that failed
      </div>
      <div class="lastline">
        <div class="item1">Ignored test cases:</div>
        <div class="item200">
          <span class="{$ignored-style}"><xsl:value-of select="@ignored-tests"/> (<xsl:value-of select="@ignored-tests-percent"/>%)</span>
        </div>
        The total number of ignored test cases
      </div>
    </div>
    <p>
      <xsl:choose>
        <xsl:when test="@has-failed='True'">Testing is not passed; <span class="alert"><xsl:value-of select="@failed-tests"/></span> failed test(s).</xsl:when>
        <xsl:otherwise>Testing is passed; no failed tests.</xsl:otherwise>
      </xsl:choose>      
    </p>
  </xsl:template>
  
  <xsl:template match="test-cases">
    <xsl:apply-templates select="//test-case" />
  </xsl:template>
  
  <xsl:template match="test-case">

    <xsl:variable name="file-name" select="@test-file" />

    <xsl:variable name="status-style">
      <xsl:choose>
        <xsl:when test="./test-result/@is-failed='True'">alert</xsl:when>
        <xsl:when test="./test-result/@is-ignored='True'">warn</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="status-text">
      <xsl:choose>
        <xsl:when test="./test-result/@is-failed='True'">FAIL</xsl:when>
        <xsl:when test="./test-result/@is-ignored='True'">IGNORED</xsl:when>
        <xsl:otherwise>PASS</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <div class="container">
      <div class="line">
        <div class="item2">Test #<xsl:value-of select="@test-index"/></div>
        <div class="item2">Case #<xsl:value-of select="@test-case-index"/></div>
        <a href="file://{$file-name}"><xsl:value-of select="@short-file-name"/></a>
        <div class="status"><span class="{$status-style}"><xsl:value-of select="$status-text" /></span> </div>
        <div class="item3">Execution time: <xsl:value-of select="./test-result/@exectime"/> ms</div>
      </div>
      <xsl:if test="$status-text='PASS'">
        <div class="lastline">
          <div class="item2">Arguments:</div>
          <xsl:value-of select="@cmd-line"/>
        </div>
      </xsl:if>
      <xsl:if test="$status-text='IGNORED'">
        <div class="line">
          <div class="item2">Arguments:</div>
          <xsl:value-of select="@cmd-line"/>
        </div>
        <div class="lastline">
          <div class="item2">Reason:</div>
          <xsl:value-of select="./test-result/@reason-to-ignore"/>
        </div>
      </xsl:if>
      <xsl:if test="$status-text='FAIL'">

        <xsl:variable name="reason-code">
          <xsl:choose>
            <xsl:when test="./test-result/@diff-in-code='True'">[Unexpected exit code] </xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="reason-err">
          <xsl:choose>
            <xsl:when test="./test-result/@diff-in-err='True'">[Unexpected error output] </xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="reason-out">
          <xsl:choose>
            <xsl:when test="./test-result/@diff-in-out='True'">[Unexpected output] </xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="reason-exception">
          <xsl:choose>
            <xsl:when test="./test-result/@has-exception='True'">(Runtime exception) </xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <div class="line">
          <div class="item2">Arguments:</div>
          <xsl:value-of select="@cmd-line"/>
        </div>
        <div class="line">
          <div class="item2">Reason(s):</div>
          <xsl:value-of select="$reason-code"/>
          <xsl:value-of select="$reason-err"/>
          <xsl:value-of select="$reason-out"/>
          <xsl:value-of select="$reason-exception"/>
        </div>
        
        <xsl:if test="./test-result/@has-exception='True'">
          <div class="line">
            <div class="item1">Exception message:</div>
            <xsl:value-of select="./test-result/@exception-message"/>
          </div>
          <div class="lastline">
            <div class="item1">Effective arguments:</div>
            <xsl:value-of select="./test-result/@arguments"/>
          </div>
        </xsl:if>

        <xsl:if test="./test-result/@has-exception!='True'">
          <div class="line">
            <div class="item1">Effective arguments:</div>
            <xsl:value-of select="./test-result/@arguments"/>
          </div>
          
          
          <div class="line">
              <div class="leftpanel">
                  Expected Results
                  <div class="item4"><xsl:value-of select="./test-result/@expected-exit-code"/></div>
                  <div class="item4">Exit Code:</div>
                </div>              
              <div class="rightpanel">
                  Actual Results
                  <div class="item4"><xsl:value-of select="./test-result/actual-exit-code/@value"/></div>
                  <div class="item4">Exit Code:</div>
              </div>
          </div>
          <div class="line">
              <div class="leftpanel">
                Output
                <pre class="content"><xsl:value-of select="./test-case-output"/></pre>
              </div>
              <div class="rightpanel">
                Output
                <pre class="content"><xsl:value-of select="./test-result/actual-output"/></pre>
              </div>
          </div>
          <div class="lastline">
              <div class="leftpanel">
                Errors
                <pre class="content"><xsl:value-of select="./test-case-err"/></pre>
              </div>
              <div class="rightpanel">
                Errors
                <pre class="content"><xsl:value-of select="./test-result/actual-err"/></pre>
              </div>
          </div>
        </xsl:if>
      </xsl:if>
    </div>
    <div class="separator"></div>
  </xsl:template>


</xsl:stylesheet>
